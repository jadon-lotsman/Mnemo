using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Contracts;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Contracts.Glossary;
using Mnemo.Contracts.Glossary.Requests;
using Mnemo.Entities;
using Mnemo.Infrastructure;
using Mnemo.Infrastructure.ErrorCodes;
using Mnemo.Infrastructure.Queries;
using Mnemo.Infrastructure.RequestResults;
using Mnemo.Shared.Enums;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.ManagementServices
{
    public class GlossaryManagementService
    {
        private readonly ILogger<GlossaryManagementService> _logger;
        private readonly IValidator<CreateEntryRequest> _createEntryValidator;
        private readonly IValidator<CreateGlossaryRequest> _createGlosssaryValidator;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly UserQueries _userQueries;
        private readonly GlossaryEntryQueries _entryQueries;
        private readonly EntryManagementService _entryService;
        private readonly GlossaryQueries _glossaryQueries;




        public GlossaryManagementService(
            ILogger<GlossaryManagementService> logger,
            IValidator<CreateEntryRequest> createEntryValidator,
            IValidator<CreateGlossaryRequest> createGlossaryValidator,
            IMapper mapper,
            AppDbContext context,
            UserQueries accountQueries,
            GlossaryEntryQueries entryQueries,
            GlossaryQueries glossaryQueries,
            EntryManagementService entryService)
        {
            _logger = logger;
            _createEntryValidator = createEntryValidator;
            _createGlosssaryValidator = createGlossaryValidator;
            _mapper = mapper;
            _context = context;
            _userQueries = accountQueries;
            _entryQueries = entryQueries;
            _glossaryQueries = glossaryQueries;
            _entryService = entryService;
        }


        public async Task<RequestResult<PageValue<HeaderResponse>>> PageGlossariesAsync(int userId, int page, int pageSize)
        {
            var messages = new List<string>();
            if (page < 1) messages.Add($"Page must be >= 1");
            if (pageSize < 1 || pageSize > 100) messages.Add($"PageSize must be in [1, 100])");

            if (messages.Count > 0)
                return RequestResult<PageValue<HeaderResponse>>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));

            
            _logger.LogDebug("Paging glossaries for user (UserId:{UserId}): page={Page}, size={Size}...", userId, page, pageSize);


            var orderedQuery = _glossaryQueries.GetByOwnerIdQuery(userId)
                .OrderBy(v => v.Name);

            var glossTotal = await orderedQuery.CountAsync();
            int totalPages = glossTotal == 0 ? 1 : (int)Math.Ceiling(glossTotal / (double)pageSize);

            if (glossTotal == 0)
                return RequestResult<PageValue<HeaderResponse>>.Success(new PageValue<HeaderResponse>(page, pageSize, totalPages, []));


            var headers = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new { v.Id, v.Name, v.Guid })
                .ToListAsync();

            var vocabIds = headers.Select(e => e.Id).ToList();

            var entryCounts = await _context.GlossaryLinks
                .Where(l => vocabIds.Contains(l.GlossaryId))
                .GroupBy(l => l.GlossaryId)
                .Select(g => new { glossId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.glossId, x => x.Count);

            var translationCounts = await _context.GlossaryLinks
                .Where(l => vocabIds.Contains(l.GlossaryId))
                .GroupBy(l => l.GlossaryId)
                .Select(g => new
                {
                    glossId = g.Key,
                    Total = g.Sum(l => l.Entry.Translations.Count)
                })
                .ToDictionaryAsync(x => x.glossId, x => x.Total);


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

        public async Task<List<GlossarySectorResponse>> GetGlossarySectorsAsync(int userId, Guid guid, bool isDescending)
        {
            var query = _entryQueries.GetByGlossaryGuidQuery(userId, guid);
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
            var sectors = new List<GlossarySectorResponse>();
            var index = 0;

            foreach (var group in groups)
            {
                string sectorStart = group.StartWord;
                string sectorEnd = group.EndWord;
                int count = group.Count;


                if (!sectors.Any())
                {
                    sectors.Add(new GlossarySectorResponse()
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
                        sectors.Add(new GlossarySectorResponse()
                        {
                            StartWord = sectorStart,
                            EndWord = sectorEnd,
                            Count = count
                        });
                    }
                }

                index++;
            }

            if (!sectors.Any())
            {
                if (isDescending)
                {
                    sectors.Add(new GlossarySectorResponse()
                    {
                        StartWord = "z",
                        EndWord = "a",
                        Count = 0
                    });
                }
                else
                {
                    sectors.Add(new GlossarySectorResponse()
                    {
                        StartWord = "a",
                        EndWord = "z",
                        Count = 0
                    });
                }
            }
            else
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

        public async Task<RequestResult<Glossary>> CreateGlossaryAsync(int userId, CreateGlossaryRequest request)
        {
            _logger.LogInformation("Creating a glossary for user (UserId:{UserId})...", userId);

            if (!await _userQueries.ExistsByIdAsync(userId))
            {
                _logger.LogWarning("User (UserId:{UserId}) not found", userId);
                return RequestResult<Glossary>.Failure(ErrorCode.UserNotFound);
            }

            var validationRequest = await _createGlosssaryValidator.ValidateAsync(request);

            if (!validationRequest.IsValid)
            {
                var messages = string.Join("; ", validationRequest.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("CreateGlossaryRequest (UserId:{UserId}) is not valid: {messages}", userId, messages);
                return RequestResult<Glossary>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }

            var validation = await _createEntryValidator.ValidateBatchAsync(request.Entries!, _logger);

            if (!validation.HasAnySuccess)
            {
                var messages = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return RequestResult<Glossary>.Failure(ErrorCode.InvalidData, messages);
            }


            var mapped = _mapper.Map<List<GlossaryEntry>>(validation.Values);

            var resolving = await _entryService.ResolveExistingEntriesAsync(userId, mapped, ExistingPolicy.ReuseExisting);


            var gloss = _mapper.Map<Glossary>(request);
            gloss.OwnerId = userId;
            gloss.Links = resolving.Values.Select(e => new GlossaryLink() { Entry = e }).ToList();

            await _context.Glossaries.AddAsync(gloss);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully created glossary (Guid:{Guid}) for user (UserId:{UserId})!", gloss.Guid, userId);

            return RequestResult<Glossary>.Success(gloss);
        }

        public async Task<RequestResult<Glossary>> ImportGlossaryAsync(int userId, Guid targetGuid, Guid sourceGuid)
        {
            _logger.LogInformation("Importing glossary (TargetGuid:{TargetGuid}) with glossary (SourceGuid:{SourceGuid}) for user (UserId:{UserId})", targetGuid, sourceGuid, userId);

            if (!await _glossaryQueries.ExistsByGuidAsync(userId, sourceGuid))
            {
                _logger.LogWarning("Source glossary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", sourceGuid, userId);
                return RequestResult<Glossary>.Failure(ErrorCode.GlossaryNotFound);
            }

            var targetGloss = await _glossaryQueries.GetByGuidAsync(userId, targetGuid);
            if (targetGloss == null)
            {
                _logger.LogWarning("Target glossary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", targetGuid, userId);
                return RequestResult<Glossary>.Failure(ErrorCode.GlossaryNotFound);
            }


            var entries = await _entryQueries.GetByGlossaryGuidQuery(userId, sourceGuid).ToListAsync();
            var linking = await _entryService.CreateLinksFromEntriesAsync(userId, targetGloss.Id, entries);

            if (!linking.HasAnySuccess)
            {
                var messages = string.Join("; ", linking.Errors.Select(e => e.ErrorMessage));
                var duplicationErrors = RequestResult<Glossary>.Failure(ErrorCode.DuplicateEntry, messages);
                return duplicationErrors;
            }


            var linksToAdd = linking.Values;

            targetGloss.Links.AddRange(linksToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully imported (TargetGuid:{TargetGuid}) with glossary (SourceGuid:{SourceGuid}) for user (UserId:{UserId})", targetGuid, sourceGuid, userId);

            return RequestResult<Glossary>.Success(targetGloss);
        }

        public async Task<RequestResult<Glossary>> PatchGlossaryAsync(int userId, Guid guid, PatchGlossaryRequest request)
        {
            _logger.LogInformation("Attempting to patch a glossary for user (UserId:{UserId})", userId);

            var currentGloss = await _glossaryQueries.GetByGuidAsync(userId, guid);
            if (currentGloss == null)
            {
                _logger.LogWarning("Glossary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<Glossary>.Failure(ErrorCode.GlossaryNotFound);
            }

            var isPatched = currentGloss.TryPatch(request);
            if (!isPatched)
            {
                _logger.LogError("TryPatch failed for glossary (Guid:{Guid}): Invalid Data", guid);
                return RequestResult<Glossary>.Failure(ErrorCode.InvalidData, "Failed to apply patch");
            }


            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully patched glossary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            return RequestResult<Glossary>.Success(currentGloss);
        }

        public async Task<RequestResult<Guid>> RevokeSharingGuidAsync(int userId, Guid guid)
        {
            _logger.LogInformation("Revoking sh-guid for glossary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            var currentGloss = await _glossaryQueries.GetByGuidAsync(userId, guid);
            if (currentGloss == null)
            {
                _logger.LogWarning("Glossary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<Guid>.Failure(ErrorCode.GlossaryNotFound);
            }


            var newGuid = Guid.NewGuid();
            currentGloss.SharingGuid = newGuid;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully revoked sh-guid for glossary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            return RequestResult<Guid>.Success(newGuid);
        }

        public async Task<RequestResult<bool>> RemoveGlossaryAsync(int userId, Guid guid)
        {
            _logger.LogInformation("Attempting to delete glossary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            var currentGloss = await _glossaryQueries.GetByGuidAsync(userId, guid);
            if (currentGloss == null)
            {
                _logger.LogWarning("Glossary (Guid:{Guid}) not found or access denied for user (UserId:{UserId})", guid, userId);
                return RequestResult<bool>.Failure(ErrorCode.GlossaryNotFound);
            }

            _context.Glossaries.Remove(currentGloss);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted glossary (Guid:{Guid}) for user (UserId:{UserId})", guid, userId);

            return RequestResult<bool>.Success(true);
        }
    }
}
