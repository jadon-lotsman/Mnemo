using AutoMapper;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Contracts.Glossary;
using Mnemo.Contracts.Glossary.Requests;
using Mnemo.Data.Entities;
using Mnemo.Shared;

namespace Mnemo.Services.Mapping
{
    public class GlossaryProfile : Profile
    {
        public GlossaryProfile()
        {
            CreateMap<CreateEntryRequest, GlossaryEntry>()
                .IncludeBase<CreateEntryRequest, VocabularyDefinition>();


            CreateMap<Glossary, GlossaryResponse>();

            CreateMap<CreateGlossaryRequest, Glossary>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => TextNormalizer.NormalizeName(src.Name)))
                .ForMember(dest => dest.Links, opt => opt.Ignore());
        }
    }
}
