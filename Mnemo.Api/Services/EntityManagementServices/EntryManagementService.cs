using AutoMapper;
using AutoMapper.Internal;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Contracts;
using Mnemo.Contracts.Entry;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Entities;
using Mnemo.Infrastructure;
using Mnemo.Infrastructure.Queries;
using Mnemo.Infrastructure.RequestResults;
using Mnemo.Infrastructure.RequestResults.ErrorCodes;
using Mnemo.Services.RepetitionService;
using Mnemo.Shared;
using Mnemo.Shared.Enums;
using Mnemo.Shared.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mnemo.Services.EntityManagementServices
{
    public class EntryManagementService
    {
        private readonly ILogger<EntryManagementService> _logger;
        private readonly IValidator<CreateEntryRequest> _createValidator;
        private readonly IValidator<PatchEntryRequest> _patchValidator;
        private readonly IMapper _mapper;
        private readonly IOptions<SM2Options> _sm2;
        private readonly AppDbContext _context;
        private readonly UserQueries _accountQueries;
        private readonly GlossaryQueries _glossaryQueries;
        private readonly GlossaryLinkQueries _linkQueries;
        private readonly GlossaryEntryQueries _entryQueries;


        public EntryManagementService(
            ILogger<EntryManagementService> logger,
            IValidator<CreateEntryRequest> createValidator,
            IValidator<PatchEntryRequest> patchValidator,
            IMapper mapper,
            IOptions<SM2Options> sm2,
            AppDbContext context,
            UserQueries accountQueries,
            GlossaryQueries vocabularyQueries,
            GlossaryLinkQueries linkQueries,
            GlossaryEntryQueries entryQueries)
        {
            _logger = logger;
            _createValidator = createValidator;
            _patchValidator = patchValidator;
            _mapper = mapper;
            _sm2 = sm2;
            _context = context;
            _accountQueries = accountQueries;
            _glossaryQueries = vocabularyQueries;
            _linkQueries = linkQueries;
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

            var lowerBound = minLetter.Substring(0, 1);
            var upperBound = maxLetter.Substring(0, 1) + char.MaxValue;

            var filteredQuery = _entryQueries
                .GetByGlossaryGuidQuery(userId, guid)
                .Where(e => string.Compare(e.Foreign, lowerBound) >= 0 &&
                            string.Compare(e.Foreign, upperBound) <= 0);

            var letterRangeTotal = await filteredQuery.CountAsync();
            int totalPages = letterRangeTotal == 0 ? 1 : (int)Math.Ceiling(letterRangeTotal / (double)pageSize);

            if (letterRangeTotal == 0)
                return RequestResult<PageValue<EntryResponse>>.Success(new PageValue<EntryResponse>(page, pageSize, 1, []));


            var orderedQuery = isDescending
                ? filteredQuery.OrderByDescending(e => e.Foreign).ThenByDescending(e => e.PartOfSpeech)
                : filteredQuery.OrderBy(e => e.Foreign).ThenBy(e => e.PartOfSpeech);

            var entries = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var entryIds = entries.Select(e => e.Id).ToList();
            var linkCounts = entryIds.Count == 0
                ? new Dictionary<int, int>()
                : await _context.GlossaryLinks
                    .Where(l => entryIds.Contains(l.EntryId))
                    .GroupBy(l => l.EntryId)
                    .Select(g => new { EntryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.EntryId, x => x.Count);

            var items = _mapper.Map<List<EntryResponse>>(entries);
            foreach (var (dto, entity) in items.Zip(entries))
                dto.LinkCount = linkCounts.GetValueOrDefault(entity.Id, 0);

            var pageValue = new PageValue<EntryResponse>(page, pageSize, totalPages, items);

            return RequestResult<PageValue<EntryResponse>>.Success(pageValue);
        }

        public async Task<BatchRequestResult<GlossaryEntry>> CreateEntriesAsync(int userId, IReadOnlyCollection<CreateEntryRequest> requests)
        {
            _logger.LogInformation("Creating {Count} entries for user (UserId:{UserId})...", requests.Count, userId);

            if (!await _accountQueries.ExistsByIdAsync(userId))
            {
                _logger.LogWarning("User (UserId:{UserId}) not found", userId);
                return BatchRequestResult<GlossaryEntry>.BatchFailure(ErrorCode.UserNotFound);
            }

            var validation = await _createValidator.ValidateBatchAsync(requests, _logger);
            var validationErr = BatchRequestResult<GlossaryEntry>.ProjectErrors(validation);

            if (!validation.HasAnySuccess)
                return validationErr;


            var mapped = _mapper.Map<List<GlossaryEntry>>(validation.Values);

            var resolving = await ResolveExistingEntriesAsync(userId, mapped, ExistingPolicy.Error);
            var resolvingErr = BatchRequestResult<GlossaryEntry>.ProjectErrors(resolving);

            if (!resolving.HasAnySuccess)
                return BatchRequestResult<GlossaryEntry>.Concat(validationErr, resolving);


            await _context.GlossaryEntries.AddRangeAsync(resolving.Values);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} entries to inbox (UserId:{UserId})!", resolving.Values.Count, userId);

            return BatchRequestResult<GlossaryEntry>.Concat(validationErr, resolving);
        }

        public async Task<BatchRequestResult<GlossaryEntry>> CreateGlossaryEntriesAsync(int userId, Guid guid, IReadOnlyCollection<CreateEntryRequest> requests)
        {
            _logger.LogInformation("Creating {Count} vocabulary entries for user (UserId:{UserId})...", requests.Count, userId);

            int? id = await _glossaryQueries.GetIdByGuidAsync(userId, guid);
            if (!id.HasValue)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})!", guid, userId);
                return BatchRequestResult<GlossaryEntry>.BatchFailure(ErrorCode.GlossaryNotFound);
            }

            var validation = await _createValidator.ValidateBatchAsync(requests, _logger);
            var validationErr = BatchRequestResult<GlossaryEntry>.ProjectErrors(validation);

            if (!validation.HasAnySuccess)
                return validationErr;


            var mapped = _mapper.Map<List<GlossaryEntry>>(validation.Values);

            var resolving = await ResolveExistingEntriesAsync(userId, mapped, ExistingPolicy.ReuseExisting);
            var resolvingErr = BatchRequestResult<GlossaryEntry>.ProjectErrors(resolving);

            if (!resolving.HasAnySuccess)
                return BatchRequestResult<GlossaryEntry>.Concat(validationErr, resolvingErr);


            var linking = await CreateLinksFromEntriesAsync(userId, id.Value, resolving.Values);
            var linkingErr = BatchRequestResult<GlossaryEntry>.ProjectErrors(linking);

            if (!linking.HasAnySuccess)
                return BatchRequestResult<GlossaryEntry>.Concat(validationErr, resolvingErr, linkingErr);


            var linkingSucc = BatchRequestResult<GlossaryEntry>.Return(linking.Values.Select(l => RequestResult<GlossaryEntry>.Success(l.Entry)).ToList());

            await _context.GlossaryLinks.AddRangeAsync(linking.Values);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} vocabulary entry (UserId:{UserId}, glossId:{glossId})!", linking.Values.Count, userId, id.Value);

            return BatchRequestResult<GlossaryEntry>.Concat(validationErr, resolvingErr, linkingSucc, linkingErr);
        }

        public async Task<BatchRequestResult<GlossaryEntry>> ResolveExistingEntriesAsync(int userId, IReadOnlyCollection<GlossaryEntry> entries, ExistingPolicy policy)
        {
            int total = entries.Count;

            _logger.LogDebug("Resolving {Count} entries (UserId: {UserId})...", total, userId);

            if (total == 0)
            {
                _logger.LogDebug("No entries (UserId: {UserId})!", userId);
                return BatchRequestResult<GlossaryEntry>.BatchFailure(ErrorCode.InvalidData);
            }

            var foreigns = entries
                .Select(e => e.Foreign)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct()
                .ToList();

            var existingEntries = await _entryQueries
                .GetKeysByForeignsAsync(userId, foreigns);

            int reused = 0;
            var results = new List<RequestResult<GlossaryEntry>>(total);
            foreach (var entry in entries)
            {
                if (existingEntries.TryGetValue((entry.Foreign, entry.PartOfSpeech), out var existingEntry))
                {
                    switch (policy)
                    {
                        case ExistingPolicy.Error:
                            results.Add(RequestResult<GlossaryEntry>.Failure(ErrorCode.DuplicateEntry, $"Entry '{entry.Foreign}' with part of speech '{entry.PartOfSpeech}' already exists"));
                            break;
                        case ExistingPolicy.ReuseExisting:
                            results.Add(RequestResult<GlossaryEntry>.Success(existingEntry));
                            reused++;
                            break;
                    }
                }
                else
                {
                    entry.OwnerId = userId;
                    entry.RepetitionState = new RepetitionState()
                    {
                        EasinessFactor = _sm2.Value.InitEF,
                        RepetitionInterval = _sm2.Value.MinInterval
                    };

                    results.Add(RequestResult<GlossaryEntry>.Success(entry));
                }
            }

            if (reused == total)
                _logger.LogInformation("Resolve completed for user (UserId:{UserId}): all {Total} entries is reused!", userId, total);
            else if (reused > 0)
                _logger.LogInformation("Resolve completed for user (UserId:{UserId}): {Reused} reused entries out of {Total}!", userId, reused, total);
            else
                _logger.LogInformation("Resolve completed for user (UserId:{UserId}): all {Total} entries is unique!", userId, total);

            return BatchRequestResult<GlossaryEntry>.Return(results);
        }

        public async Task<BatchRequestResult<GlossaryLink>> CreateLinksFromEntriesAsync(int userId, int glossId, IReadOnlyCollection<GlossaryEntry> entries)
        {
            entries = entries
                .GroupBy(e => (e.Foreign, e.PartOfSpeech))
                .Select(g => g.First())
                .ToList();

            int total = entries.Count;

            _logger.LogDebug("Linking {Count} entries (UserId: {UserId}, glossId: {glossId})...", total, userId, glossId);

            if (total == 0)
            {
                _logger.LogDebug("No entries (UserId: {UserId}, glossId: {glossId})!", userId, glossId);
                return BatchRequestResult<GlossaryLink>.BatchFailure(ErrorCode.InvalidData);
            }


            var entryIds = entries.Where(e => e.Id > 0).Select(e => e.Id).ToList();
            var linkedIds = await _entryQueries
                .GetAlreadyLinkedIdsAsync(glossId, entryIds);

            int duplicates = 0;
            var results = new List<RequestResult<GlossaryLink>>(total);
            foreach (var entry in entries)
            {
                if (!linkedIds.Contains(entry.Id))
                {
                    var link = new GlossaryLink()
                    {
                        GlossaryId = glossId,
                        Entry = entry
                    };

                    results.Add(RequestResult<GlossaryLink>.Success(link));
                    continue;
                }

                results.Add(RequestResult<GlossaryLink>.Failure(ErrorCode.DuplicateEntry, $"Entry '{entry.Foreign}' with part of speech '{entry.PartOfSpeech}' already linked"));
                duplicates++;
            }

            if (duplicates == total)
                _logger.LogWarning("Set link completed (UserId:{UserId}, glossId:{glossId}): all {Total} entries are diplicates!", userId, glossId, total);
            else if (duplicates > 0)
                _logger.LogInformation("Set link completed (UserId:{UserId}, glossId:{glossId}): {Failed} duplicates out of {Total}!", userId, glossId, duplicates, total);
            else
                _logger.LogDebug("Set link completed (UserId:{UserId}, glossId:{glossId}): all {Total} entries are unique!", userId, glossId, total);

            return BatchRequestResult<GlossaryLink>.Return(results);
        }

        public async Task<RequestResult<GlossaryEntry>> PatchEntryAsync(int userId, Guid guid, int entryId, PatchEntryRequest request)
        {
            _logger.LogInformation("Patching entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            if (!await _glossaryQueries.ExistsByGuidAsync(userId, guid))
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<GlossaryEntry>.Failure(ErrorCode.GlossaryNotFound);
            }

            var validationResult = await _patchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("PatchEntryRequest (EntryId:{EntryId}) is not valid: {messages}", entryId, messages);
                return RequestResult<GlossaryEntry>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }


            var currentEntry = await _entryQueries.GetByIdAsync(userId, entryId);
            if (currentEntry == null)
            {
                _logger.LogWarning("Entry (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);
                return RequestResult<GlossaryEntry>.Failure(ErrorCode.EntryNotFound);
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
                    return RequestResult<GlossaryEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");
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
                return RequestResult<GlossaryEntry>.Failure(ErrorCode.InvalidData, "Failed to apply patch");
            }


            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully patched entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            return RequestResult<GlossaryEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> RemoveLinkAsync(int userId, Guid guid, int entryId)
        {
            _logger.LogInformation("Removing link for entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            int? id = await _glossaryQueries.GetIdByGuidAsync(userId, guid);
            if (!id.HasValue)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<bool>.Failure(ErrorCode.GlossaryNotFound);
            }

            var currentLink = await _linkQueries.GetByPairIdAsync(id.Value, entryId);
            if (currentLink == null)
            {
                _logger.LogWarning("Link (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);
                return RequestResult<bool>.Failure(ErrorCode.LinkNotFound);
            }


            _context.GlossaryLinks.Remove(currentLink);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            return RequestResult<bool>.Success(true);
        }

        public async Task<RequestResult<bool>> RemoveAllLinksAsync(int userId, int entryId)
        {
            _logger.LogInformation("Removing all links for entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            if (!await _entryQueries.ExistsByIdAsync(userId, entryId))
            {
                _logger.LogWarning("Entry (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);
                return RequestResult<bool>.Failure(ErrorCode.EntryNotFound);
            }

            var allLinks = await _linkQueries.GetLinksByEntryIdAsync(entryId);
            if (allLinks.Count == 0)
            {
                _logger.LogInformation("Any links not found for entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);
                return RequestResult<bool>.Failure(ErrorCode.LinkNotFound);
            }

            _context.GlossaryLinks.RemoveRange(allLinks);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted {Count} links for entry (EntryId:{EntryId}) for user (UserId:{UserId})", allLinks.Count, entryId, userId);

            return RequestResult<bool>.Success(true);
        }

        public async Task<RequestResult<bool>> RemoveEntryAsync(int userId, int entryId)
        {
            _logger.LogInformation("Attempting to delete entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            var inboxEntry = await _entryQueries.GetInboxedByIdAsync(userId, entryId);
            if (inboxEntry != null)
            {
                _context.GlossaryEntries.Remove(inboxEntry);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

                return RequestResult<bool>.Success(true);
            }

            if (!await _entryQueries.ExistsByIdAsync(userId, entryId))
            {
                _logger.LogWarning("Entry (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);

                return RequestResult<bool>.Failure(ErrorCode.EntryNotFound);
            }

            _logger.LogWarning("Entry (EntryId:{EntryId}) not cannot inbox for user (UserId:{UserId})", entryId, userId);
            return RequestResult<bool>.Failure(ErrorCode.EntryHasLinks);
        }
    }
}
