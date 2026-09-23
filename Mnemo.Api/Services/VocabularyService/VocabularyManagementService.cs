using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Contracts;
using Mnemo.Contracts.Entry;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;
using Mnemo.Shared.Enums;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.VocabularyService
{
    public class VocabularyManagementService
    {
        private readonly ILogger<VocabularyManagementService> _logger;
        private readonly IValidator<CreateVocabularyRequest> _createVocabularyValidator;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly AccountQueries _accountQueries;
        private readonly VocabularyEntryQueries _entryQueries;
        private readonly VocabularyQueries _vocabularyQueries;
        private readonly EntryManagementService _entryService;




        public VocabularyManagementService(
            ILogger<VocabularyManagementService> logger,
            IValidator<CreateVocabularyRequest> createVocabularyValidator,
            IMapper mapper,
            AppDbContext context,
            AccountQueries accountQueries,
            VocabularyEntryQueries entryQueries,
            VocabularyQueries vocabularyQueries,
            EntryManagementService entryService)
        {
            _logger = logger;
            _createVocabularyValidator = createVocabularyValidator;
            _mapper = mapper;
            _context = context;
            _accountQueries = accountQueries;
            _entryQueries = entryQueries;
            _vocabularyQueries = vocabularyQueries;
            _entryService = entryService;
        }


        public async Task<RequestResult<PageValue<HeaderResponse>>> PageUserHeadersAsync(int userId, int page, int pageSize)
        {
            var messages = new List<string>();
            if (page < 1) messages.Add($"Page must be >= 1");
            if (pageSize < 1 || pageSize > 100) messages.Add($"PageSize must be in [1, 100])");

            if (messages.Count > 0)
                return RequestResult<PageValue<HeaderResponse>>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));

            
            _logger.LogDebug("Paging vocabularies for user (UserId:{UserId}): page={Page}, size={Size}...", userId, page, pageSize);


            var orderedQuery = _vocabularyQueries.GetVocabByOwnerIdQuery(userId)
                .OrderBy(v => v.Name);

            var vocabsTotal = await orderedQuery.CountAsync();
            int totalPages = vocabsTotal == 0 ? 1 : (int)Math.Ceiling(vocabsTotal / (double)pageSize);

            if (vocabsTotal == 0)
                return RequestResult<PageValue<HeaderResponse>>.Success(new PageValue<HeaderResponse>(page, pageSize, totalPages, []));


            var headers = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new { v.Id, v.Name, v.Guid })
                .ToListAsync();

            var vocabIds = headers.Select(e => e.Id).ToList();

            var entryCounts = await _context.VocabularyEntryLinks
                .Where(l => vocabIds.Contains(l.VocabularyId))
                .GroupBy(l => l.VocabularyId)
                .Select(g => new { VocabId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.VocabId, x => x.Count);

            var translationCounts = await _context.VocabularyEntryLinks
                .Where(l => vocabIds.Contains(l.VocabularyId))
                .GroupBy(l => l.VocabularyId)
                .Select(g => new
                {
                    VocabId = g.Key,
                    Total = g.Sum(l => l.VocabularyEntry.Translations.Count)
                })
                .ToDictionaryAsync(x => x.VocabId, x => x.Total);


            var items = headers.Select(v => new HeaderResponse
            {
                Name = v.Name,
                Guid = v.Guid,
                EntriesCount = entryCounts.GetValueOrDefault(v.Id, 0),
                TranslationsCount = translationCounts.GetValueOrDefault(v.Id, 0)
            }).ToList();


            var pageValue = new PageValue<HeaderResponse>(page, pageSize, totalPages, items);

            return RequestResult<PageValue<HeaderResponse>>.Success(pageValue);
        }

        public async Task<List<VocabularySectorResponse>> GetVocabularySectorsAsync(int userId, Guid guid, bool isDescending)
        {
            var query = _entryQueries.GetEntriesByVocabularyGuidQuery(userId, guid);
            int minSectorSize = Math.Max(10, query.Count() / 7);

            var groupQuery = query
                .GroupBy(e => e.Foreign.Substring(0, 1))
                .Select(g => new
                {
                    Letter = g.Key,
                    Count = g.Count(),
                    StartWord = g.Min(e => e.Foreign)!,
                    EndWord = g.Max(e => e.Foreign)!
                })
                .OrderBy(e => e.Letter);


            var groups = await groupQuery.ToListAsync();
            var sectors = new List<VocabularySectorResponse>();
            var index = 0;

            foreach (var group in groups)
            {
                string sectorStart = group.StartWord;
                string sectorEnd = group.EndWord;
                int count = group.Count;


                if (!sectors.Any())
                {
                    sectors.Add(new VocabularySectorResponse()
                    {
                        StartWord = sectorStart,
                        EndWord = sectorEnd,
                        Count = count
                    });
                }
                else
                {
                    var lastSection = sectors.Last();
                    var isLastGroup = index == groups.Count - 1;

                    if (lastSection.Count < minSectorSize || (isLastGroup && count < minSectorSize))
                    {
                        lastSection.EndWord = sectorEnd;
                        lastSection.Count += count;
                    }
                    else
                    {
                        sectors.Add(new VocabularySectorResponse()
                        {
                            StartWord = sectorStart,
                            EndWord = sectorEnd,
                            Count = count
                        });
                    }
                }

                index++;
            }

            if (sectors.Any())
            {
                sectors.First().StartWord = "a";
                sectors.Last().EndWord = "z";

                if (isDescending)
                {
                    foreach (var sector in sectors)
                        (sector.EndWord, sector.StartWord) = (sector.StartWord, sector.EndWord);

                    sectors.Reverse();
                }
            }


            return sectors;
        }

        public async Task<RequestResult<Vocabulary>> CreateVocabularyAsync(int userId, CreateVocabularyRequest request)
        {
            _logger.LogInformation("Creating a vocabulary for user (UserId:{UserId})...", userId);

            var validationResult = await _createVocabularyValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("CreateVocabularyRequest (UserId:{UserId}) is not valid: {messages}", userId, messages);
                return RequestResult<Vocabulary>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }

            if (!await _accountQueries.ExistsByIdAsync(userId))
            {
                _logger.LogWarning("User (UserId:{UserId}) not found", userId);
                return RequestResult<Vocabulary>.Failure(ErrorCode.UserNotFound);
            }

            var linksToAdd = new List<VocabularyEntryLink>();

            if (!request.Entries.IsNullOrEmpty())
            {
                var linkResults = await _entryService.SetVocabularyLinksAsync(userId, null, request.Entries);

                if (linkResults.IsAllFailure)
                {
                    var messages = string.Join("; ", linkResults.FailedResults.Select(e => e.ErrorMessage));
                    var duplicationErrors = RequestResult<Vocabulary>.Failure(ErrorCode.DuplicateEntry, messages);
                    return duplicationErrors;
                }

                linksToAdd = linkResults.SucceededResults.Select(r => r.Value!).ToList();
            }

            var vocab = _mapper.Map<Vocabulary>(request);
            vocab.OwnerId = userId;
            vocab.EntryLinks = linksToAdd;


            await _context.Vocabularies.AddAsync(vocab);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully created vocabulary (Guid:{Guid}) for user (UserId:{UserId})!", vocab.Guid, userId);

            return RequestResult<Vocabulary>.Success(vocab);
        }

        public async Task<RequestResult<Vocabulary>> MergeVocabularyAsync(int userId, Guid targetGuid, Guid sourceGuid)
        {
            _logger.LogInformation("Starting merge vocabulary (TargetGuid:{TargetGuid}) with vocabulary (SourceGuid:{SourceGuid}) for user (UserId:{UserId})", targetGuid, sourceGuid, userId);

            if (!await _vocabularyQueries.ExistsByIdAsync(userId, sourceGuid))
            {
                _logger.LogWarning("Source vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", sourceGuid, userId);
                return RequestResult<Vocabulary>.Failure(ErrorCode.VocabularyNotFound);
            }

            var targetVocab = await _vocabularyQueries.GetByGuidAsync(userId, targetGuid);
            if (targetVocab == null)
            {
                _logger.LogWarning("Target vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", targetGuid, userId);
                return RequestResult<Vocabulary>.Failure(ErrorCode.VocabularyNotFound);
            }


            var entries = await _entryQueries.GetEntriesByVocabularyGuidQuery(userId, sourceGuid).ToListAsync();
            var linkResults = await _entryService.SetVocabularyLinksAsync(userId, targetVocab.Id, entries);


            var messages = string.Join("; ", linkResults.FailedResults.Select(e => e.ErrorMessage));
            var duplicationErrors = RequestResult<Vocabulary>.Failure(ErrorCode.DuplicateEntry, messages);

            if (linkResults.IsAllFailure)
                return duplicationErrors;


            var linksToAdd = linkResults.SucceededResults.Select(r => r.Value!);

            targetVocab.EntryLinks.AddRange(linksToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully merged (TargetGuid:{TargetGuid}) with vocabulary (SourceGuid:{SourceGuid}) for user (UserId:{UserId})", targetGuid, sourceGuid, userId);

            return RequestResult<Vocabulary>.Success(targetVocab);
        }

        public async Task<RequestResult<Vocabulary>> PatchVocabularyAsync(int userId, Guid guid, PatchVocabularyRequest request)
        {
            _logger.LogInformation("Attempting to patch a vocabulary for user (UserId:{UserId})", userId);

            var currentVocab = await _vocabularyQueries.GetByGuidAsync(userId, guid);
            if (currentVocab == null)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<Vocabulary>.Failure(ErrorCode.VocabularyNotFound);
            }

            var isPatched = currentVocab.TryPatch(request);
            if (!isPatched)
            {
                _logger.LogError("TryPatch failed for vocabulary (Guid:{Guid}): Invalid Data", guid);
                return RequestResult<Vocabulary>.Failure(ErrorCode.InvalidData, "Failed to apply patch");
            }


            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully patched vocabulary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            return RequestResult<Vocabulary>.Success(currentVocab);
        }

        public async Task<RequestResult<Guid>> RevokeVocabularyGuidAsync(int userId, Guid guid)
        {
            _logger.LogInformation("Attempting to revoke guid for vocabulary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            var currentVocab = await _vocabularyQueries.GetByGuidAsync(userId, guid);
            if (currentVocab == null)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<Guid>.Failure(ErrorCode.VocabularyNotFound);
            }


            var newGuid = Guid.NewGuid();
            currentVocab.Guid = newGuid;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully revoked guid for vocabulary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            return RequestResult<Guid>.Success(newGuid);
        }

        public async Task<RequestResult<bool>> RemoveVocabularyByGuidAsync(int userId, Guid guid)
        {
            _logger.LogInformation("Attempting to delete vocabulary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            var currentVocab = await _vocabularyQueries.GetByGuidAsync(userId, guid);
            if (currentVocab == null)
            {
                _logger.LogWarning("Vocabulary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<bool>.Failure(ErrorCode.VocabularyNotFound);
            }


            _context.Vocabularies.Remove(currentVocab);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted vocabulary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            return RequestResult<bool>.Success(true);
        }
    }
}
