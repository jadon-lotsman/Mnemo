using System.Text.Json.Serialization;
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
                .Include<OrderPartsRepetitionTask, OrderPartsTaskResponse>()
                .Include<YesOrNoRepetitionTask, YesOrNoTaskResponse>();

            CreateMap<TextRepetitionTask, TextTaskResponse>();
            CreateMap<OptionRepetitionTask, OptionTaskResponse>();
            CreateMap<OrderPartsRepetitionTask, OrderPartsTaskResponse>();
            CreateMap<YesOrNoRepetitionTask, YesOrNoTaskResponse>();
        }
    }
}
