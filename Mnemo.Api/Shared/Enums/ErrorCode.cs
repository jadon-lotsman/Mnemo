namespace Mnemo.Shared.Enums
{
    public enum ErrorCode
    {
        // BadRequest
        InvalidData,
        InvalidPassword,

        // NotFound
        UserNotFound,
        VocabularyNotFound,
        EntryNotFound,
        StateNotFound,
        RepetitionNotFound,
        TaskNotFound,

        // Conflict/Dublicate
        EntryHasLinks,
        UsernameTaken,
        DuplicateEntry,

        // UnprocessableEntity
        TaskGenerationFailed,
        ExternalDictionaryError,
    }
}
