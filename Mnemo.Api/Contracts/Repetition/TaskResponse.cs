using System.Text.Json.Serialization;

namespace Mnemo.Contracts.Repetition
{
    [JsonDerivedType(typeof(TextTaskResponse), typeDiscriminator: "text")]
    [JsonDerivedType(typeof(OptionTaskResponse), typeDiscriminator: "option")]
    [JsonDerivedType(typeof(OrderPartsTaskResponse), typeDiscriminator: "parts")]
    [JsonDerivedType(typeof(YesOrNoTaskResponse), typeDiscriminator: "yesorno")]
    public abstract class TaskResponse
    {
        public int Id { get; set; }
        public abstract string TaskType { get; }
        public string Prompt { get; set; }
    }

    public class TextTaskResponse : TaskResponse
    {
        public override string TaskType => "text";
    }

    public class OptionTaskResponse : TaskResponse
    {
        public override string TaskType => "option";
        public List<string> Options { get; set; }
    }

    public class OrderPartsTaskResponse : TaskResponse
    {
        public override string TaskType => "parts";
        public List<string> SentenceParts { get; set; }
    }

    public class YesOrNoTaskResponse : TaskResponse
    {
        public override string TaskType => "yesorno";
    }
}
