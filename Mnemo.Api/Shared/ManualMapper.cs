using Mnemo.Contracts.Repetition;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data.Entities;
using Mnemo.Shared.Extensions;

namespace Mnemo.Shared
{
    public static class ManualMapper
    {
        public static bool ValidDto(CreateEntryRequest? dto)
        {
            return dto != null && dto.Foreign != string.Empty && dto.Translations != null && dto.Translations.Any(t => !string.IsNullOrWhiteSpace(t));
        }

        public static bool ValidDto(PatchEntryRequest? dto)
        {
            if (dto == null) return false;
            var properties = dto.GetType().GetProperties();
            return properties.Any(p => p.GetValue(dto) != null);
        }
    }
}
