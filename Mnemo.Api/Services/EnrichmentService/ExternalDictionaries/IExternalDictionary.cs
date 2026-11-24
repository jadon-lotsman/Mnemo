using Mnemo.Contracts.Entry;
using Mnemo.Infrastructure.RequestResults;
using Mnemo.Shared.Enums;

namespace Mnemo.Services.EnrichmentService.ExternalDictionaries
{
    public interface IExternalDictionary
    {
        Task<RequestResult<EnrichResponse?>> GetEnrichAsync(string foreign, PartOfSpeech? partOfSpeech);
    }
}
