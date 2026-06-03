using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;

namespace Mnemo.Services
{
    public class VocabularyManagementService
    {
        private AppDbContext _context;
        private AccountQueries _accountQueries;
        private VocabularyQueries _vocabularyQueries;


        public VocabularyManagementService(AppDbContext context, AccountQueries accountQueries, VocabularyQueries vocabularyQueries)
        {
            _context = context;
            _accountQueries = accountQueries;
            _vocabularyQueries = vocabularyQueries;
        }



        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, CreateEntryRequest dto)
        {
            if (!ManualMapper.ValidDto(dto))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData);

            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.UserNotFound);


            string foreign = ManualMapper.PrepareForeign(dto.Foreign!);

            if (await _vocabularyQueries.ExistsByForeignAsync(userId, foreign))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");


            var entry = ManualMapper.MapToEntry(dto, userId);

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, PatchEntryRequest patchDto)
        {
            if (!ManualMapper.ValidDto(patchDto))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData);


            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.EntryNotFound);


            ManualMapper.PatchFromDto(currentEntry, patchDto);

            await _context.SaveChangesAsync();

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
