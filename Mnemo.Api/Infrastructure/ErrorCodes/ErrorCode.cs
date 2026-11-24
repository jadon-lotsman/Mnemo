namespace Mnemo.Infrastructure.ErrorCodes
{
    public enum ErrorCode
    {
        // BadRequest
        InvalidData,
        InvalidPassword,

        // NotFound
        UserNotFound,
        GlossaryNotFound,
        EntryNotFound,
        LinkNotFound,
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
