using Mnemo.Services.RepetitionService.TaskObjects.TaskTypes;
using System.Text.Json.Serialization;

namespace Mnemo.Contracts.Repetition.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AcceptFlashcardTaskRequest), "flashcard")]
    [JsonDerivedType(typeof(AcceptTextInputTaskRequest), "text_input")]
    [JsonDerivedType(typeof(AcceptSingleChoiceTaskRequest), "single_choice")]
    public abstract class AcceptTaskRequest
    {
        public Guid Nonce { get; set; }
        public Guid EntryId { get; set; }
        public long IssuedAt { get; set; }
        public required string Signature { get; set; }
    }

    public class AcceptFlashcardTaskRequest : AcceptTaskRequest
    {
        public int QualityRate { get; set; }
    }

    public class AcceptTextInputTaskRequest : AcceptTaskRequest
    {
        public required string TextInput { get; set; }
    }

    public class AcceptSingleChoiceTaskRequest : AcceptTaskRequest
    {
        public int AnswerIndex { get; set; }
    }
}
