using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data.Entities;

namespace Mnemo.Shared.Extensions
{
    public static class VocabularyEntryExtension
    {
        public static RequestResult<bool> Patch(this VocabularyEntry entry, PatchEntryRequest patch)
        {
            if (!string.IsNullOrWhiteSpace(patch.Foreign))
            {
                string normalized = TextNormalizer.NormalizeForeign(patch.Foreign);

                if (string.IsNullOrWhiteSpace(normalized) || !normalized.Any(char.IsLetter))
                    return RequestResult<bool>.Failure(ErrorCode.InvalidData, $"Foreign '{patch.Foreign}' is invalid");

                entry.Foreign = normalized;
            }

            if (!string.IsNullOrWhiteSpace(patch.Transcription))
            {
                string normalized = TextNormalizer.NormalizeTranscription(patch.Transcription);

                if (string.IsNullOrWhiteSpace(normalized))
                    return RequestResult<bool>.Failure(ErrorCode.InvalidData, $"Transcription '{patch.Transcription}' is invalid");

                entry.Transcription = normalized;
            }


            if (patch.ExamplesAdd != null)
            {
                var toAddNormalized = TextNormalizer.NormalizeEnumerable(patch.ExamplesAdd, TextNormalizer.NormalizeExample);
                entry.Examples.AddRange(toAddNormalized);
            }
            if (patch.ExamplesRemove != null)
            {
                var toRemoveNormalized = TextNormalizer.NormalizeEnumerable(patch.ExamplesRemove, TextNormalizer.NormalizeExample);
                entry.Examples.RemoveAll(toRemoveNormalized.Contains);
            }


            if (patch.TranslationsAdd != null)
            {
                var toAddNormalized = TextNormalizer.NormalizeEnumerable(patch.TranslationsAdd, TextNormalizer.NormalizeTranslation);
                entry.Translations.AddRange(toAddNormalized);
            }
            if (patch.TranslationsRemove != null)
            {
                var toRemoveNormalized = TextNormalizer.NormalizeEnumerable(patch.TranslationsRemove, TextNormalizer.NormalizeTranslation);
                entry.Translations.RemoveAll(toRemoveNormalized.Contains);
            }

            return RequestResult<bool>.Success(true);
        }
    }
}
