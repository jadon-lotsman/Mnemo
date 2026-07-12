using System.Text.Json.Serialization;

namespace Mnemo.Contracts.Repetition
{
    [JsonDerivedType(typeof(TextTaskResponse), typeDiscriminator: "text")]
    [JsonDerivedType(typeof(OptionTaskResponse), typeDiscriminator: "option")]
    [JsonDerivedType(typeof(SentenceReorderTaskResponse), typeDiscriminator: "sentence")]
    [JsonDerivedType(typeof(SyllableReorderTaskResponse), typeDiscriminator: "syllable")]
    [JsonDerivedType(typeof(YesOrNoTaskResponse), typeDiscriminator: "yesorno")]
    public abstract class TaskResponse
    {
        public int Id { get; set; }
        public string? PartOfSpeech { get; set; }
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

    public class SentenceReorderTaskResponse : TaskResponse
    {
        public override string TaskType => "sentence";
        public List<string> SentenceParts { get; set; }
    }

    public class SyllableReorderTaskResponse : TaskResponse
    {
        public override string TaskType => "syllable";
        public List<string> Syllables { get; set; }
    }

    public class YesOrNoTaskResponse : TaskResponse
    {
        public override string TaskType => "yesorno";
        public string Option { get; set; }
    }
}
