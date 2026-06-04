using Mnemo.Shared.Extensions;

namespace Mnemo.Shared
{
    public static class TextNormalizer
    {
        private static string NormalizeCore(string? str)
            => str?.RemoveMultispaces().ToLowerInvariant() ?? string.Empty;

        public static string NormalizeForeign(string? str)
            => NormalizeCore(str);

        public static string NormalizeTranscription(string? str)
            => NormalizeCore(str).WrapWithBracketsIfNeeded();

        public static string NormalizeExample(string? str)
            => str?.RemoveMultispaces().Capitalize().AddEndPointIfNeeded() ?? string.Empty;

        public static string NormalizeTranslation(string? str)
            => NormalizeCore(str);

        public static List<string> NormalizeEnumerable(IEnumerable<string>? items, Func<string, string> normalize)
        {
            if (items == null) return [];
            return items
                .Select(i => i?.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => normalize(i!))
                .Distinct()
                .ToList();
        }
    }
}
