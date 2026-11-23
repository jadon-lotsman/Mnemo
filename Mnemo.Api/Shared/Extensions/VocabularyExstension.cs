using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Shared.Extensions
{
    public static class VocabularyExstension
    {
        public static bool TryPatch(this Vocabulary vocab, PatchVocabularyRequest patch)
        {
            string? _name = null;
            if (patch.Name != null)
            {
                var normalized = vocab.Name.RemoveMultispaces().Capitalize();
                if (string.IsNullOrWhiteSpace(normalized))
                    return false;

                _name = normalized;
            }


            if (_name != null)
                vocab.Name = _name;

            return true;
        }
    }
}
