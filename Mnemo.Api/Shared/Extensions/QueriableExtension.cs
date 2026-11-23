using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;

namespace Mnemo.Shared.Extensions
{
    public static class QueriableExtension
    {
        public static IQueryable<GlossaryEntry> DueEntries(this IQueryable<GlossaryEntry> source)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return source
            .Where(e => e.RepetitionState != null &&
                        e.RepetitionState.NextRepetitionAt <= today);
        }

        public static IQueryable<GlossaryEntry> NotDueEntries(this IQueryable<GlossaryEntry> source)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return source
            .Where(e => e.RepetitionState != null
                     && e.RepetitionState.NextRepetitionAt > today);
        }

        public static IQueryable<GlossaryEntry> NotRepeatedTodayEntries(this IQueryable<GlossaryEntry> source)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return source
            .Where(e => e.RepetitionState != null
                    && e.RepetitionState.LastRepetitionAt < today);
        }

        public static IQueryable<GlossaryEntry> GetRandomEntries(this IQueryable<GlossaryEntry> source, int? take = null, params int[] excludeIds)
        {
            var query = source;

            if (excludeIds != null && excludeIds.Any())
                query = query.Where(e => !excludeIds.Contains(e.Id));

            if (take.HasValue)
                return query
                    .OrderBy(e => EF.Functions.Random())
                    .Take(take.Value);

            return query
            .OrderBy(e => EF.Functions.Random());
        }
    }
}
