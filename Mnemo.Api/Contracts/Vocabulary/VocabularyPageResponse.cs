namespace Mnemo.Contracts.Vocabulary
{
    public class VocabularyPageResponse
    {
        public EntryResponse[]? Entries { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalEntries { get; set; }
        public int TotalTranslations { get; set; }
    }
}
