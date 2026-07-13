using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.VocabularyService
{
    public class VocabularyManagementService
    {
        private readonly ILogger<VocabularyManagementService> _logger;
        private readonly IValidator<CreateEntryRequest> _createValidator;
        private readonly IValidator<PatchEntryRequest> _patchValidator;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly AccountQueries _accountQueries;
        private readonly VocabularyQueries _vocabularyQueries;


        public VocabularyManagementService(
            ILogger<VocabularyManagementService> logger,
            IValidator<CreateEntryRequest> createValidator,
            IValidator<PatchEntryRequest> patchValidator,
            IMapper mapper,
            AppDbContext context,
            AccountQueries accountQueries,
            VocabularyQueries vocabularyQueries)
        {
            _logger = logger;
            _createValidator = createValidator;
            _patchValidator = patchValidator;
            _mapper = mapper;
            _context = context;
            _accountQueries = accountQueries;
            _vocabularyQueries = vocabularyQueries;
        }



        public async Task<List<VocabularySectorResponse>> GetVocabularySectorsAsync(int userId)
        {
            int minSectorSize = 12;

            var alphabetGroups = await _vocabularyQueries
                .GetByUserIdQuery(userId)
                .GroupBy(e => e.Foreign.Substring(0, 1))
                .Select(g => new
                {
                    Letter = g.Key,
                    Count = g.Count(),
                    StartWord = g.Min(e => e.Foreign)!,
                    EndWord = g.Max(e => e.Foreign)!
                })
                .OrderBy(e => e.Letter)
                .ToListAsync();

            var sectors = new List<VocabularySectorResponse>();

            while (alphabetGroups.Any())
            {
                string startWord = alphabetGroups.First().StartWord;
                string endWord = alphabetGroups.First().EndWord;
                int count = alphabetGroups.First().Count;


                if (!sectors.Any())
                {
                    sectors.Add(new VocabularySectorResponse()
                    {
                        StartWord = startWord,
                        EndWord = endWord,
                        Count = count
                    });
                }
                else
                {
                    var lastSection = sectors.Last();

                    if (lastSection.Count < minSectorSize)
                    {

                        lastSection.EndWord = endWord;
                        lastSection.Count += count;
                    }
                    else
                    {
                        sectors.Add(new VocabularySectorResponse()
                        {
                            StartWord = startWord,
                            EndWord = endWord,
                            Count = count
                        });
                    }
                }

                alphabetGroups.RemoveAt(0);
            }


            return sectors;
        }

        public async Task<VocabularyPageResponse> GetVocabularyPageAsync(int userId, string startWord, string endWord)
        {
            var totalEntries = await _vocabularyQueries.GetByUserIdQuery(userId)
                .CountAsync();

            var totalTranslations = await _vocabularyQueries.GetByUserIdQuery(userId)
                .SumAsync(e => e.Translations != null ? e.Translations.Count : 0);

            var entries = await _vocabularyQueries
                .GetByUserIdQuery(userId)
                .Where(e => 
                string.Compare(e.Foreign, startWord) >= 0 && 
                string.Compare(e.Foreign, endWord) <= 0)
                .OrderBy(e => e.Foreign)
                .ThenBy(e => e.PartOfSpeech)
                //.Skip((page - 1) * pageSize)
                //.Take(pageSize)
                .ToListAsync();
                            

            var entriesResponse = _mapper.Map<EntryResponse[]>(entries);

            return new VocabularyPageResponse
            {
                Entries = entriesResponse,
                TotalEntries = totalEntries,
                TotalTranslations = totalTranslations
            };
        }


        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, CreateEntryRequest request)
        {
            _logger.LogInformation("Attempting to create vocabulary entry for user (UserId:{UserId}): Foreign:{Foreign}, PartOfSpeech:{PartOfSpeech}", userId, request.Foreign, request.PartOfSpeech ?? "without(null)");

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("CreateEntryRequest (UserId:{UserId}) is not valid: {messages}", userId, messages);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, messages);
            }


            if (!await _accountQueries.ExistsByIdAsync(userId))
            {
                _logger.LogWarning("User (UserId:{UserId}) not found", userId);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.UserNotFound);
            }


            var entry = _mapper.Map<VocabularyEntry>(request);

            if (await _vocabularyQueries.ExistsByKeysAsync(userId, entry.Foreign, entry.PartOfSpeech))
            {
                _logger.LogWarning("Duplicate entry for user (UserId:{UserId}): Foreign:{Foreign}, PartOfSpeech:{PartOfSpeech}", userId, entry.Foreign, entry.PartOfSpeech?.ToString() ?? "without(null)");
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");
            }


            entry.UserId = userId;
            entry.RepetitionState = new RepetitionState();

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created vocabulary entry (EntryId:{EntryId}) for user (UserId:{UserId})", entry.Id, userId);

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, PatchEntryRequest request)
        {
            _logger.LogInformation("Patching entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            var validationResult = await _patchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("PatchEntryRequest (EntryId:{EntryId}) is not valid: {messages}", entryId, messages);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }


            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);
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

                if (await _vocabularyQueries.ExistsByKeysAsync(currentEntry.UserId, checkForeign, checkPartOfSpeech))
                {
                    _logger.LogWarning("Duplicate check failed for entry (EntryId:{EntryId})", entryId);
                    return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");
                }
            }


            if (foreignUpdated || partOfSpeechUpdated)
            {
                currentEntry.ResetAllMeta();
                _logger.LogInformation("All metadata reset and set as {Status}: (EntryId:{EntryId}) for user (UserId:{UserId})", currentEntry.EnrichmentStatus, entryId, userId);
            }
            else if (transcriptionUpdated)
            {
                currentEntry.ResetAudio();
                _logger.LogInformation("Audio reset and set as {Status}: (EntryId:{EntryId}) for user (UserId:{UserId})", currentEntry.EnrichmentStatus, entryId, userId);
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

        public async Task<RequestResult<bool>> RemoveEntryByIdAsync(int userId, int entryId)
        {
            _logger.LogInformation("Attempting to delete entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
            {
                _logger.LogWarning("Entry (EntryId:{EntryId}) not found for user (UserId:{UserId})", entryId, userId);
                return RequestResult<bool>.Failure(ErrorCode.EntryNotFound);
            }


            _context.Entries.Remove(currentEntry);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted entry (EntryId:{EntryId}) for user (UserId:{UserId})", entryId, userId);

            return RequestResult<bool>.Success(true);
        }
    }
}
