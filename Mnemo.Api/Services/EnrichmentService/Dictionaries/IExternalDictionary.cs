using Mnemo.Contracts.Vocabulary;
using Mnemo.Shared;

namespace Mnemo.Services.EnrichmentService.Dictionaries
{
    public interface IExternalDictionary
    {
        Task<RequestResult<EnrichResponse>> GetEnrichAsync(string foreign, PartOfSpeech partOfSpeech);
    }
}
