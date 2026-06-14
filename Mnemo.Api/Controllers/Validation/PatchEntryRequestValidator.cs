using FluentValidation;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Shared;

namespace Mnemo.Controllers.Validation
{
    public class PatchEntryRequestValidator : AbstractValidator<PatchEntryRequest>
    {
        public PatchEntryRequestValidator()
        {
            RuleFor(x => x)
                .Must(AtLeastOneFieldSpecified)
                .WithMessage("At least one field must be provided for update");


            RuleFor(x => x.PartOfSpeech)
                .Must(VocabularyValidatorRules.IsValidPartOfSpeech)
                .When(x => x.PartOfSpeech != null)
                .WithMessage($"Invalid Part of Speech. Allowed values: {string.Join(", ", Enum.GetNames<PartOfSpeech>())}");

            RuleFor(x => x.Foreign)
                .Must(VocabularyValidatorRules.IsValidForeign)
                .When(x => x.Foreign != null)
                .WithMessage("Foreign must contain only letters, spaces, apostrophes, hyphens (no digits or special chars)");

            RuleFor(x => x.Transcription)
                .Must(VocabularyValidatorRules.IsValidTranscription)
                .When(x => x.Transcription != null)
                .WithMessage("Transcription format is invalid");

            RuleForEach(x => x.TranslationsAdd)
                .Must(VocabularyValidatorRules.IsValidTranslation)
                .When(x => x.TranslationsAdd != null)
                .WithMessage("Each translation must be valid (no digits, must contain letters)");

            RuleForEach(x => x.ExamplesAdd)
                .Must(VocabularyValidatorRules.IsValidExample)
                .When(x => x.ExamplesAdd != null)
                .WithMessage("Example too short or invalid");
        }

        private bool AtLeastOneFieldSpecified(PatchEntryRequest request)
        {
            return request.Foreign != null ||
                   request.PartOfSpeech != null ||
                   request.Transcription != null ||
                   (request.ExamplesAdd?.Any() == true) ||
                   (request.ExamplesRemove?.Any() == true) ||
                   (request.TranslationsAdd?.Any() == true) ||
                   (request.TranslationsRemove?.Any() == true) ||
                   (request.SynonymsAdd?.Any() == true) ||
                   (request.SynonymsRemove?.Any() == true) ||
                   (request.AntonymsAdd?.Any() == true) ||
                   (request.AntonymsRemove?.Any() == true);
        }
    }
}
