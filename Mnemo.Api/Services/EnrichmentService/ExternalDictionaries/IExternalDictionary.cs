using Mnemo.Contracts.Vocabulary;
using Mnemo.Shared;
using Mnemo.Shared.Enums;

namespace Mnemo.Services.EnrichmentService.ExternalDictionaries
{
    public interface IExternalDictionary
    {
        Task<RequestResult<EnrichResponse?>> GetEnrichAsync(string foreign, PartOfSpeech? partOfSpeech);
    }
}
