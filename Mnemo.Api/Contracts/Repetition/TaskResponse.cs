using Microsoft.Extensions.Options;
using Mnemo.Services.RepetitionService.TaskObjects.TaskTypes;
using Mnemo.Shared.Enums;
using System.Security.Cryptography.Xml;
using System.Text.Json.Serialization;

namespace Mnemo.Contracts.Repetition
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(FlashcardTaskResponse), "flashcard")]
    [JsonDerivedType(typeof(TextInputTaskResponse), "text_input")]
    [JsonDerivedType(typeof(SingleChoiceTaskResponse), "single_choice")]
    public abstract class TaskResponse
    {
        public string Prompt { get; set; }
        public PartOfSpeech? PromptPos { get; set; }

        public Guid Nonce { get; set; }
        public Guid EntryId { get; set; }
        public long IssuedAt { get; set; }
        public string Signature { get; set; } // HMAC(userId, entryId, taskTypeInteger, issuedAt, nonce) 


        public TaskResponse(string prompt, PartOfSpeech? partOfSpeech, Guid nonce, Guid entryId, long issuedAt, string signature)
        {
            Prompt = prompt;
            PromptPos = partOfSpeech;
            Nonce = nonce;
            EntryId = entryId;
            IssuedAt = issuedAt;
            Signature = signature;
        }
    }

    public class FlashcardTaskResponse : TaskResponse
    {
        public string Answer { get; set; }

        public FlashcardTaskResponse(
            string prompt,
            PartOfSpeech? partOfSpeech,
            string answer,
            Guid nonce,
            Guid entryId,
            long issuedAt,
            string signature) : base(prompt, partOfSpeech, nonce, entryId, issuedAt, signature)
        {
            Answer = answer;
        }
    }

    public class TextInputTaskResponse : TaskResponse
    {
        public TextInputTaskResponse(string prompt,
            PartOfSpeech? partOfSpeech,
            Guid nonce,
            Guid entryId,
            long issuedAt,
            string signature) : base(prompt, partOfSpeech, nonce, entryId, issuedAt, signature)
        {

        }
    }

    public class SingleChoiceTaskResponse : TaskResponse
    {
        public string[] Options { get; set; } = [];

        public SingleChoiceTaskResponse(
            string prompt,
            PartOfSpeech? partOfSpeech,
            string[] options,
            Guid nonce,
            Guid entryId,
            long issuedAt,
            string signature) : base(prompt, partOfSpeech, nonce, entryId, issuedAt, signature)
        {
            Options = options;
        }
    }
}
