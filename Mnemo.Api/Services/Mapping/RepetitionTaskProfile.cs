using AutoMapper;
using Mnemo.Contracts.Repetition;
using Mnemo.Data.Entities;

namespace Mnemo.Services.Mapping
{
    public class RepetitionTaskProfile : Profile
    {
        public RepetitionTaskProfile()
        {
            CreateMap<RepetitionTask, TaskResponse>()
                .Include<TextRepetitionTask, TextTaskResponse>()
                .Include<OptionRepetitionTask, OptionTaskResponse>()
                .Include<SentenceReorderRepetitionTask, SentenceReorderTaskResponse>()
                .Include<SyllableReorderRepetitionTask, SyllableReorderTaskResponse>()
                .Include<YesOrNoRepetitionTask, YesOrNoTaskResponse>()
                .ForMember(dest => dest.PartOfSpeech, opt => opt.MapFrom(src => src.EntryPartOfSpeech.HasValue ? src.EntryPartOfSpeech.Value.ToString().ToLower() : null)); ;

            CreateMap<TextRepetitionTask, TextTaskResponse>();
            CreateMap<OptionRepetitionTask, OptionTaskResponse>();
            CreateMap<SentenceReorderRepetitionTask, SentenceReorderTaskResponse>();
            CreateMap<SyllableReorderRepetitionTask, SyllableReorderTaskResponse>();
            CreateMap<YesOrNoRepetitionTask, YesOrNoTaskResponse>();
        }
    }
}
