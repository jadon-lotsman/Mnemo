using System.Threading;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data;
using Mnemo.Data.Queries;
using Mnemo.Shared;

namespace Mnemo.Services.EnrichmentService.Dictionaries
{
    public class FreeDictionaryApiService : IExternalDictionary
    {
        private readonly ILogger<VocabularyManagementService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;


        public FreeDictionaryApiService(ILogger<VocabularyManagementService> logger, HttpClient httpClient, IMemoryCache cache)
        {
            _logger = logger;
            _httpClient = httpClient;
            _cache = cache;
        }


        public async Task<RequestResult<EnrichResponse>> GetEnrichAsync(string foreign, PartOfSpeech partOfSpeech)
        {
            _logger.LogInformation("Attempting to get enrich entry: Foreign:{Foreign}, PartOfSpeech:{PartOfSpeech}", foreign, partOfSpeech);

            if (string.IsNullOrWhiteSpace(foreign))
            {
                _logger.LogWarning("Foreign word is required");
                return RequestResult<EnrichResponse>.Failure(ErrorCode.InvalidData);
            }

            var cacheKey = $"{foreign.ToLowerInvariant()} as {partOfSpeech.ToString().ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out EnrichResponse cached))
            {
                _logger.LogInformation("Return from cache (Foreign:{Foreign}, PartOfSpeech:{PartOfSpeech})", foreign, partOfSpeech);
                return RequestResult<EnrichResponse>.Success(cached);
            }


            try
            {
                var result = await _httpClient.GetAsync($"https://api.dictionaryapi.dev/api/v2/entries/en/{foreign}");

                if (!result.IsSuccessStatusCode)
                    return RequestResult<EnrichResponse>.Failure(ErrorCode.InvalidData);

                var entries = await result.Content.ReadFromJsonAsync<List<FreeDictionaryEntry>>();

                if (entries == null || !entries.Any())
                    return RequestResult<EnrichResponse>.Failure(ErrorCode.InvalidData);


                var entry = entries[0];

                Phonetic? phonetic = null;
                Meaning? partOfSpeechMeaning = null;

                if (entry.Phonetics != null)
                {
                    phonetic = entry.Phonetics.Where(p => !string.IsNullOrWhiteSpace(p.Text) && !string.IsNullOrWhiteSpace(p.Audio) && p.Audio.Contains("-us.mp3")).FirstOrDefault();

                    if (phonetic == null)
                    {
                        var onlyText = entry.Phonetics.Where(p => !string.IsNullOrWhiteSpace(p.Text)).FirstOrDefault();
                        var onlyAudio = entry.Phonetics.Where(p => !string.IsNullOrWhiteSpace(p.Audio) && p.Audio.Contains("-us.mp3")).FirstOrDefault();

                        if (onlyText != null)
                        {
                            phonetic = new Phonetic()
                            {
                                Text = onlyText!.Text,
                                Audio = onlyAudio?.Audio,
                            };
                        }
                    }
                }

                if (entry.Meanings != null)
                {
                    partOfSpeechMeaning = entry.Meanings.Where(m => m.PartOfSpeech == partOfSpeech.ToString().ToLowerInvariant()).FirstOrDefault();
                }

                var enrichResponse = new EnrichResponse()
                {
                    Transcription = phonetic?.Text,
                    TranscriptionAudioUrl = phonetic?.Audio,
                    Synonyms = partOfSpeechMeaning?.Synonyms?.ToArray(),
                    Antonyms = partOfSpeechMeaning?.Antonyms?.ToArray(),
                };

                _cache.Set(cacheKey, enrichResponse, TimeSpan.FromHours(12));

                return RequestResult<EnrichResponse>.Success(enrichResponse);
            }
            catch (Exception ex)
            {
                return RequestResult<EnrichResponse>.Failure(ErrorCode.InvalidData, ex.Message);
            }
        }


        public class FreeDictionaryEntry
        {
            public string? Word { get; set; }
            public string? Phonetic { get; set; }
            public List<Phonetic>? Phonetics { get; set; }
            public List<Meaning>? Meanings { get; set; }
        }

        public class Phonetic
        {
            public string? Text { get; set; }
            public string? Audio { get; set; }
        }

        public class Meaning
        {
            public string? PartOfSpeech { get; set; }
            public List<string>? Synonyms { get; set; }
            public List<string>? Antonyms { get; set; }
        }
    }
}
