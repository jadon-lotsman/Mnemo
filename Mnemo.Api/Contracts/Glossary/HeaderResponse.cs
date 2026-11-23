namespace Mnemo.Contracts.Glossary
{
    public class HeaderResponse
    {
        public string? Name { get; set; }
        public Guid? Guid { get; set; }
        public int EntriesCount { get; set; }
        public int TranslationsCount { get; set; }
    }
}
