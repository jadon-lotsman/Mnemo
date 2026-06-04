using AutoMapper;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data.Entities;
using Mnemo.Shared;

namespace Mnemo.Services.Mapping
{
    public class VocabularyEntryProfile : Profile
    {
        public VocabularyEntryProfile()
        {
            CreateMap<VocabularyEntry, EntryResponse>();

            CreateMap<CreateEntryRequest, VocabularyEntry>()
                .ForMember(dest => dest.Foreign, opt => opt.MapFrom(src => TextNormalizer.NormalizeForeign(src.Foreign)))
                .ForMember(dest => dest.Transcription, opt => opt.MapFrom(src => TextNormalizer.NormalizeTranscription(src.Transcription)))
                .ForMember(dest => dest.Examples, opt => opt.MapFrom(src => TextNormalizer.NormalizeEnumerable(src.Examples, TextNormalizer.NormalizeExample)))
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => TextNormalizer.NormalizeEnumerable(src.Translations, TextNormalizer.NormalizeTranslation)));
        }
    }
}
