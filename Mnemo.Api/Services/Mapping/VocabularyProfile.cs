using AutoMapper;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data.Entities;
using Mnemo.Shared;

namespace Mnemo.Services.Mapping
{
    public class VocabularyProfile : Profile
    {
        public VocabularyProfile()
        {
            CreateMap<CreateEntryRequest, VocabularyEntry>()
                .IncludeBase<CreateEntryRequest, VocabularyDefinition>();


            CreateMap<Vocabulary, VocabularyResponse>();

            CreateMap<CreateVocabularyRequest, Vocabulary>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextNormalizer.NormalizeName(src.Name)))
                .ForMember(dest => dest.EntryLinks, opt => opt.Ignore());
        }
    }
}
