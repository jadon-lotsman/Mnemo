using AutoMapper;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services
{
    public class VocabularyManagementService
    {
        private IMapper _mapper;
        private AppDbContext _context;
        private AccountQueries _accountQueries;
        private VocabularyQueries _vocabularyQueries;


        public VocabularyManagementService(IMapper mapper, AppDbContext context, AccountQueries accountQueries, VocabularyQueries vocabularyQueries)
        {
            _mapper = mapper;
            _context = context;
            _accountQueries = accountQueries;
            _vocabularyQueries = vocabularyQueries;
        }



        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, CreateEntryRequest request)
        {
            if (!ManualMapper.ValidDto(request))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData);

            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.UserNotFound);

            var entry = _mapper.Map<VocabularyEntry>(request);

            if (await _vocabularyQueries.ExistsByForeignAsync(userId, entry.Foreign))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");


            entry.UserId = userId;
            entry.RepetitionState = new RepetitionState();

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, PatchEntryRequest request)
        {
            if (!ManualMapper.ValidDto(request))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData);

            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.EntryNotFound);


            var result = currentEntry.Patch(request);
            await _context.SaveChangesAsync();

            if (!result.IsSuccess)
                return RequestResult<VocabularyEntry>.Failure(result.ErrorCode!.Value, result.ErrorMessage);

            return RequestResult<VocabularyEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> RemoveEntryByIdAsync(int userId, int entryId)
        {
            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<bool>.Failure(ErrorCode.EntryNotFound);


            _context.Entries.Remove(currentEntry);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
