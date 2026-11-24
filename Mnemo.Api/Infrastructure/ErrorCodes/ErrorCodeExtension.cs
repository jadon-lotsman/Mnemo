namespace Mnemo.Infrastructure.ErrorCodes
{
    public static class ErrorCodeExtension
    {
        public static int ToHttpCode(this ErrorCode? errorCode) => errorCode switch
        {
            // BadRequest
            ErrorCode.InvalidData       => StatusCodes.Status400BadRequest,
            ErrorCode.InvalidPassword   => StatusCodes.Status400BadRequest,
            // NotFound
            ErrorCode.UserNotFound          => StatusCodes.Status404NotFound,
            ErrorCode.GlossaryNotFound    => StatusCodes.Status404NotFound,
            ErrorCode.EntryNotFound         => StatusCodes.Status404NotFound,
            ErrorCode.LinkNotFound          => StatusCodes.Status404NotFound,
            ErrorCode.StateNotFound         => StatusCodes.Status404NotFound,
            ErrorCode.RepetitionNotFound    => StatusCodes.Status404NotFound,
            ErrorCode.TaskNotFound          => StatusCodes.Status404NotFound,
            // Conflict/Duplicate
            ErrorCode.EntryHasLinks     => StatusCodes.Status409Conflict,
            ErrorCode.UsernameTaken     => StatusCodes.Status409Conflict,
            ErrorCode.DuplicateEntry    => StatusCodes.Status409Conflict,
            // UnprocessableEntity
            ErrorCode.TaskGenerationFailed      => StatusCodes.Status422UnprocessableEntity,
            ErrorCode.ExternalDictionaryError   => StatusCodes.Status422UnprocessableEntity,

            _ => StatusCodes.Status418ImATeapot
        };
    }
}
