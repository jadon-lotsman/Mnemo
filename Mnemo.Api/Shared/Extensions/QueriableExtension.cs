using Mnemo.Data.Entities;

namespace Mnemo.Shared.Extensions
{
    public static class QueriableExtension
    {
        public static IQueryable<VocabularyEntry> DueEntries(this IQueryable<VocabularyEntry> source)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return source
            .Where(e => e.RepetitionState != null &&
                        e.RepetitionState.NextRepetitionAt <= today);
        }

        public static IQueryable<VocabularyEntry> NotDueEntries(this IQueryable<VocabularyEntry> source)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return source
            .Where(e => e.RepetitionState != null
                     && e.RepetitionState.NextRepetitionAt > today);
        }

        public static IQueryable<VocabularyEntry> NotRepeatedTodayEntries(this IQueryable<VocabularyEntry> source)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return source
            .Where(e => e.RepetitionState != null
                     && e.RepetitionState.LastRepetitionAt < today);
        }
    }
}
