using Mnemo.Contracts.Entry.Requests;

namespace Mnemo.Contracts.Glossary.Requests
{
    public class CreateGlossaryRequest
    {
        public string? Name { get; set; }
        public CreateEntryRequest[]? Entries { get; set; }
    }
}
