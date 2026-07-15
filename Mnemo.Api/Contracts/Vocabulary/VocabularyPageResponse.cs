namespace Mnemo.Contracts.Vocabulary
{
    public class VocabularyPageResponse
    {
        public EntryResponse[]? Entries { get; set; }
        public bool hasMore { get; set; }
        public int SectorEntries { get; set; }
    }
}
