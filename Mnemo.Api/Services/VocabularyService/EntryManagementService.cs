using AutoMapper;
using AutoMapper.Internal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Contracts;
using Mnemo.Contracts.Entry;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService;
using Mnemo.Shared;
using Mnemo.Shared.Enums;
using Mnemo.Shared.Extensions;
using System.Diagnostics;

namespace Mnemo.Services.VocabularyService
{
    public class EntryManagementService
    {
        private readonly ILogger<EntryManagementService> _logger;
        private readonly IValidator<CreateEntryRequest> _createValidator;
        private readonly IValidator<PatchEntryRequest> _patchValidator;
        private readonly IMapper _mapper;
        private readonly IOptions<SM2Options> _sm2;
        private readonly AppDbContext _context;
        private readonly VocabularyQueries _vocabularyQueries;
        private readonly VocabularyEntryQueries _entryQueries;


        public EntryManagementService(
            ILogger<EntryManagementService> logger,
            IValidator<CreateEntryRequest> createValidator,
            IValidator<PatchEntryRequest> patchValidator,
            IMapper mapper,
            IOptions<SM2Options> sm2,
            AppDbContext context,
            VocabularyQueries vocabularyQueries,
            VocabularyEntryQueries entryQueries)
        {
            _logger = logger;
            _createValidator = createValidator;
            _patchValidator = patchValidator;
            _mapper = mapper;
            _sm2 = sm2;
            _context = context;
            _vocabularyQueries = vocabularyQueries;
            _entryQueries = entryQueries;
        }



        public async Task<RequestResult<PageValue<EntryResponse>>> PageEntriesAsync(int userId, Guid guid, string startLetter, string endLetter, int page, int pageSize)
        {
            var messages = new List<string>();
            if (page < 1) messages.Add($"Page must be >= 1");
            if (pageSize < 1 || pageSize > 100) messages.Add($"PageSize must be in [1, 100]");

            if (messages.Count > 0)
                return RequestResult<PageValue<EntryResponse>>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));


            bool isDescending = string.Compare(endLetter, startLetter, StringComparison.OrdinalIgnoreCase) < 0;

            var (minLetter, maxLetter) = isDescending
                    ? (endLetter, startLetter)
                    : (startLetter, endLetter);

            _logger.LogDebug("Paging entries for user (UserId:{UserId}): letters [{Min}..{Max}], desc={Desc}, page={Page}, size={Size}...", userId, minLetter, maxLetter, isDescending, page, pageSize);

            var filteredQuery = _entryQueries
                .GetEntriesByVocabularyGuidQuery(userId, guid)
                .Where(e => string.Compare(e.Foreign, minLetter) >= 0 &&
                            string.Compare(e.Foreign, maxLetter) <= 0);

            var orderedQuery = isDescending
                ? filteredQuery.OrderByDescending(e => e.Foreign).ThenByDescending(e => e.PartOfSpeech)
                : filteredQuery.OrderBy(e => e.Foreign).ThenBy(e => e.PartOfSpeech);

            var letterRangeTotal = await orderedQuery.CountAsync();
            int totalPages = letterRangeTotal == 0 ? 1 : (int)Math.Ceiling(letterRangeTotal / (double)pageSize);

            if (totalPages == 0)
                return RequestResult<PageValue<EntryResponse>>.Success(new PageValue<EntryResponse>(page, pageSize, 1, []));


            var entries = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var entryIds = entries.Select(e => e.Id).ToList();
            var linkCounts = entryIds.Count == 0
                ? new Dictionary<int, int>()
                : await _context.VocabularyEntryLinks
                    .Where(l => entryIds.Contains(l.VocabularyEntryId))
                    .GroupBy(l => l.VocabularyEntryId)
                    .Select(g => new { EntryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.EntryId, x => x.Count);

            var items = _mapper.Map<List<EntryResponse>>(entries);
            foreach (var (dto, entity) in items.Zip(entries))
                dto.LinkCount = linkCounts.GetValueOrDefault(entity.Id, 0);

            var pageValue = new PageValue<EntryResponse>(page, pageSize, totalPages, items);

            return RequestResult<PageValue<EntryResponse>>.Success(pageValue);
        }


        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, Guid guid, CreateEntryRequest request)
        {
            var result = await CreateEntriesAsync(userId, guid, new List<CreateEntryRequest>() { request });
            return result.SucceededResults.FirstOrDefault() ?? result.FailedResults.First();
        }

        public async Task<BatchRequestResult<VocabularyEntry>> CreateEntriesAsync(int userId, Guid guid, List<CreateEntryRequest> requests)
        {
            _logger.LogInformation("Creating {Count} vocabulary entries for user (UserId:{UserId})...", requests.Count, userId);

            int? id = await _vocabularyQueries.GetIdByGuidAsync(userId, guid);
            if (!id.HasValue)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})!", guid, userId);
                return BatchRequestResult<VocabularyEntry>.BatchFailure(ErrorCode.VocabularyNotFound);
            }

            var linkResults = await SetVocabularyLinksAsync(userId, id.Value, requests);

            if (linkResults.IsAllFailure)
            {
                var messages = string.Join("; ", linkResults.FailedResults.Select(e => e.ErrorMessage));
                var linkErrors = BatchRequestResult<VocabularyEntry>.BatchFailure(ErrorCode.InvalidData, messages);
                return linkErrors;
            }


            var linksToAdd = linkResults.SucceededResults.Select(r => r.Value!);

            await _context.VocabularyEntryLinks.AddRangeAsync(linksToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} vocabulary entry (UserId:{UserId}, VocabId:{VocabId})!", linkResults.SucceededResults.Count, userId, id.Value);

            return BatchRequestResult<VocabularyEntry>.Project(linkResults, r => r.VocabularyEntry);
        }

        public async Task<BatchRequestResult<VocabularyEntryLink>> SetVocabularyLinksAsync(int userId, int? vocabId, IReadOnlyCollection<CreateEntryRequest>? requests)
        {
            if (requests == null || requests.Count == 0)
            {
                _logger.LogDebug("No requests (UserId: {UserId}, VocabId: {VocabId})!", userId, vocabId);
                return BatchRequestResult<VocabularyEntryLink>.BatchFailure(ErrorCode.InvalidData);
            }

            var validationResults = await _createValidator.ValidateBatchAsync(requests, _logger);

            var messages = string.Join("; ", validationResults.FailedResults.Select(e => e.ErrorMessage));
            var validationErrors = BatchRequestResult<VocabularyEntryLink>.BatchFailure(ErrorCode.InvalidData, messages);

            if (validationResults.IsAllFailure)
                return validationErrors;


            var succeedRequests = validationResults.SucceededResults.Select(r => r.Value!);
            var entries = _mapper.Map<List<VocabularyEntry>>(succeedRequests);

            var linkResults = await SetVocabularyLinksAsync(userId, vocabId, entries);

            var results = validationErrors.FailedResults.Concat(linkResults.Results).ToList();
            return BatchRequestResult<VocabularyEntryLink>.Return(results);
        }

        public async Task<BatchRequestResult<VocabularyEntryLink>> SetVocabularyLinksAsync(int userId, int? vocabId, IReadOnlyCollection<VocabularyEntry> entries)
        {
            entries = entries
                .RemoveKeyDuplicates()
                .ToList();
            int total = entries.Count;

            _logger.LogDebug("Starting set link for {Count} entries (UserId: {UserId}, VocabId: {VocabId})...", total, userId, vocabId);

            if (total == 0)
            {
                _logger.LogDebug("No entries (UserId: {UserId}, VocabId: {VocabId})!", userId, vocabId);
                return BatchRequestResult<VocabularyEntryLink>.BatchFailure(ErrorCode.InvalidData);
            }


            var foreigns = entries
                .Select(e => e.Foreign)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct()
                .ToList();

            var existingEntries = await _entryQueries
                .GetKeysByForeignsAsync(userId, foreigns);

            HashSet<int>? linkedEntries = null;
            if (vocabId.HasValue && vocabId != 0)
            {
                var existingIds = existingEntries.Values.Select(v => v.Id).ToList();

                linkedEntries = await _entryQueries
                    .GetAlreadyLinkedIdsAsync(vocabId.Value, existingIds);
            }


            var results = new List<RequestResult<VocabularyEntryLink>>(total);
            foreach (var entry in entries)
            {
                RequestResult<VocabularyEntryLink> result;
                if (existingEntries.TryGetValue((entry.Foreign, entry.PartOfSpeech), out var existingEntry))
                {
                    if (linkedEntries != null && linkedEntries.Contains(existingEntry.Id))
                    {
                        _logger.LogWarning("Duplicate entry detected (UserId: {UserId}, VocabId: {VocabId}): Foreign:{Foreign}, PartOfSpeech:{PartOfSpeech}", userId, vocabId, entry.Foreign, entry.PartOfSpeech);
                        result = RequestResult<VocabularyEntryLink>.Failure(ErrorCode.DuplicateEntry, $"Entry '{entry.Foreign}' with part of speech '{entry.PartOfSpeech}' already exists");
                    }
                    else
                    {
                        var addLink = new VocabularyEntryLink()
                        {
                            VocabularyId = vocabId.HasValue ? vocabId.Value : 0,
                            VocabularyEntry = existingEntry,
                        };

                        result = RequestResult<VocabularyEntryLink>.Success(addLink);
                    }
                }
                else
                {
                    var newEntry = VocabularyEntry.CreateFromDefinition(entry);
                    newEntry.OwnerId = userId;
                    newEntry.RepetitionState = new RepetitionState()
                    {
                        EasinessFactor = _sm2.Value.InitEF,
                        RepetitionInterval = _sm2.Value.MinInterval
                    };

                    var addLink = new VocabularyEntryLink()
                    {
                        VocabularyId = vocabId.HasValue ? vocabId.Value : 0,
                        VocabularyEntry = newEntry
                    };

                    result = RequestResult<VocabularyEntryLink>.Success(addLink);
                }

                results.Add(result);
            }

            int succeeded = results.Count(r => r.IsSuccess);
            int failed = total - succeeded;

            if (failed == total)
                _logger.LogWarning("Set link completed (UserId:{UserId}, VocabId:{VocabId}): all {Total} entries are diplicates!", userId, vocabId, total);
            else if (failed > 0)
                _logger.LogInformation("Set link completed (UserId:{UserId}, VocabId:{VocabId}): {Succeeded} unique, {Failed} duplicates out of {Total}!", userId, vocabId, succeeded, failed, total);
            else
                _logger.LogDebug("Set link completed (UserId:{UserId}, VocabId:{VocabId}): all {Total} entries are unique!", userId, vocabId, total);


            return BatchRequestResult<VocabularyEntryLink>.Return(results);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, Guid guid, int entryId, PatchEntryRequest request)
        {
            _logger.LogInformation("Patching entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            if (!await _vocabularyQueries.ExistsByIdAsync(userId, guid))
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.VocabularyNotFound);
            }

            var validationResult = await _patchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("PatchEntryRequest (EntryId:{EntryId}) is not valid: {messages}", entryId, messages);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }


            var currentEntry = await _entryQueries.GetByIdAsync(userId, entryId);
            if (currentEntry == null)
            {
                _logger.LogWarning("Entry (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.EntryNotFound);
            }


            PartOfSpeech? newPartOfSpeech = null;
            if (request.PartOfSpeech != null)
                newPartOfSpeech = Enum.Parse<PartOfSpeech>(request.PartOfSpeech, true);

            string? newForeign = null;
            if (request.Foreign != null)
                newForeign = TextNormalizer.NormalizeForeign(request.Foreign);

            string? newTranscription = null;
            if (request.Transcription != null)
                newTranscription = TextNormalizer.NormalizeTranscription(request.Transcription);

            bool foreignUpdated = (newForeign != null && newForeign != currentEntry.Foreign);
            bool partOfSpeechUpdated = (newPartOfSpeech != null && newPartOfSpeech.Value != currentEntry.PartOfSpeech);
            bool transcriptionUpdated = (newTranscription != null && newTranscription != currentEntry.Transcription);

            bool needDuplicateCheck = foreignUpdated || partOfSpeechUpdated;


            if (needDuplicateCheck)
            {
                var checkForeign = newForeign ?? currentEntry.Foreign;
                var checkPartOfSpeech = newPartOfSpeech ?? currentEntry.PartOfSpeech;

                if (await _entryQueries.ExistsByKeyAsync(userId, checkForeign, checkPartOfSpeech))
                {
                    _logger.LogWarning("Duplicate check failed for entry (EntryId:{EntryId})", entryId);
                    return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");
                }
            }


            if (foreignUpdated || partOfSpeechUpdated)
            {
                currentEntry.ResetAllMeta();
                _logger.LogDebug("All metadata reset and set as {Status}: (EntryId:{EntryId}) for user (UserId:{UserId})", currentEntry.EnrichmentStatus, entryId, userId);
            }
            else if (transcriptionUpdated)
            {
                currentEntry.ResetAudio();
                _logger.LogDebug("Audio reset and set as {Status}: (EntryId:{EntryId}) for user (UserId:{UserId})", currentEntry.EnrichmentStatus, entryId, userId);
            }


            var isPatched = currentEntry.TryPatch(request);

            if (!isPatched)
            {
                _logger.LogError("TryPatch failed for entry (EntryId:{EntryId}): Invalid Data", entryId);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, "Failed to apply patch");
            }


            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully patched entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            return RequestResult<VocabularyEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> RemoveEntryByIdAsync(int userId, Guid guid, int entryId)
        {
            _logger.LogInformation("Attempting to delete entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            int? id = await _vocabularyQueries.GetIdByGuidAsync(userId, guid);
            if (!id.HasValue)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<bool>.Failure(ErrorCode.VocabularyNotFound);
            }

            var currentEntry = await _entryQueries.GetByIdAsync(userId, entryId);
            if (currentEntry == null)
            {
                _logger.LogWarning("Entry (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);
                return RequestResult<bool>.Failure(ErrorCode.EntryNotFound);
            }


            _context.VocabularyEntries.Remove(currentEntry);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            return RequestResult<bool>.Success(true);
        }
    }
}
