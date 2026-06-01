namespace Mnemo.Shared
{
    public enum ErrorCode
    {
        // 400 BadRequest
        InvalidData,
        InvalidPassword,

        // 403 Forbidden
        ActionNotAllowed,

        // 404 NotFound
        UserNotFound,
        EntryNotFound,
        StateNotFound,
        TaskNotFound,
        SessionNotFound,

        // 409 Conflict/Dublicate
        UsernameTaken,
        DuplicateEntry,
        DuplicateSession,

        // 422 UnprocessableEntity
        TaskGenerationFailed
    }
}
