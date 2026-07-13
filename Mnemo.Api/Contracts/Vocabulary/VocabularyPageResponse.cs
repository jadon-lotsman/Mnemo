namespace Mnemo.Contracts.Vocabulary
{
    public class VocabularyPageResponse
    {
        public EntryResponse[]? Entries { get; set; }
        public int TotalEntries { get; set; }
        public int TotalTranslations { get; set; }
    }
}
