using AutoMapper;
using FluentValidation;
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
        private readonly IValidator<CreateEntryRequest> _createValidator;
        private readonly IValidator<PatchEntryRequest> _patchValidator;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly AccountQueries _accountQueries;
        private readonly VocabularyQueries _vocabularyQueries;


        public VocabularyManagementService(
            IValidator<CreateEntryRequest> createValidator,
            IValidator<PatchEntryRequest> patchValidator,
            IMapper mapper,
            AppDbContext context,
            AccountQueries accountQueries,
            VocabularyQueries vocabularyQueries)
        {
            _createValidator = createValidator;
            _patchValidator = patchValidator;
            _mapper = mapper;
            _context = context;
            _accountQueries = accountQueries;
            _vocabularyQueries = vocabularyQueries;
        }



        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, CreateEntryRequest request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(e => e.ErrorMessage);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }


            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.UserNotFound);


            var entry = _mapper.Map<VocabularyEntry>(request);

            if (await _vocabularyQueries.ExistsByKeysAsync(userId, entry.Foreign, entry.PartOfSpeech))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");


            entry.UserId = userId;
            entry.RepetitionState = new RepetitionState();

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, PatchEntryRequest request)
        {
            var validationResult = await _patchValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(e => e.ErrorMessage);
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, string.Join("; ", messages));
            }


            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.EntryNotFound);


            PartOfSpeech? newPartOfSpeech = null;
            if (request.PartOfSpeech != null)
                newPartOfSpeech = Enum.Parse<PartOfSpeech>(request.PartOfSpeech, true);

            string? newForeign = null;
            if (request.Foreign != null)
                newForeign = TextNormalizer.NormalizeForeign(request.Foreign);

            var checkResult = await CheckPatchDuplicateAsync(currentEntry, newForeign, newPartOfSpeech);

            if (!checkResult.IsSuccess)
                return RequestResult<VocabularyEntry>.Failure(checkResult.ErrorCode!.Value, checkResult.ErrorMessage);


            var isPatched = currentEntry.TryPatch(request);

            if (!isPatched)
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData, "Failed to apply patch");


            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> CheckPatchDuplicateAsync(VocabularyEntry currentEntry, string? newForeign, PartOfSpeech? newPartOfSpeech)
        {
            bool needDuplicateCheck = (newForeign != null && newForeign != currentEntry.Foreign) ||
                     (newPartOfSpeech != null && newPartOfSpeech.Value != currentEntry.PartOfSpeech);

            if (!needDuplicateCheck)
                return RequestResult<bool>.Success(true);


            var checkForeign = newForeign ?? currentEntry.Foreign;
            var checkPartOfSpeech = newPartOfSpeech ?? currentEntry.PartOfSpeech;

            if (await _vocabularyQueries.ExistsByKeysAsync(currentEntry.UserId, checkForeign, checkPartOfSpeech))
                return RequestResult<bool>.Failure(ErrorCode.DuplicateEntry, "Entry already exists");


            return RequestResult<bool>.Success(true);
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
