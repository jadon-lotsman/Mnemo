using Mnemo.Shared.Extensions;

namespace Mnemo.Shared
{
    public class RequestResult<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public ErrorCode? ErrorCode { get; }
        public string? ErrorMessage { get; }


        private RequestResult(T value)
        {
            IsSuccess = true;
            Value = value;
        }

        private RequestResult(ErrorCode errorCode, string? errorMessage)
        {
            IsSuccess = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage ?? errorCode.ToString().SplitCamelCase();
        }


        public static RequestResult<T> Success(T value) => new RequestResult<T>(value);
        public static RequestResult<T> Failure(ErrorCode errorCode, string? errorMessage = null) => new RequestResult<T>(errorCode, errorMessage);
    }
}
