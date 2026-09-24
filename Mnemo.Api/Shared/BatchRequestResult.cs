using Microsoft.AspNetCore.Http.HttpResults;
using Mnemo.Shared.Enums;

namespace Mnemo.Shared
{
    public class BatchRequestResult<T>
    {
        public bool IsAllFailure { get; }

        public IReadOnlyList<RequestResult<T>> Results { get; }
        public IReadOnlyList<RequestResult<T>> SucceededResults { get; }
        public IReadOnlyList<RequestResult<T>> FailedResults { get; }


        public BatchRequestResult(IReadOnlyList<RequestResult<T>> results)
        {
            Results = results;
            SucceededResults = results.Where(r => r.IsSuccess).ToList();
            FailedResults = results.Where(r => !r.IsSuccess).ToList();
            IsAllFailure = SucceededResults.Count == 0;
        }

        public BatchRequestResult(ErrorCode errorCode, string? errorMessage)
        {
            var failure = RequestResult<T>.Failure(errorCode, errorMessage);
            Results = [failure];
            SucceededResults = [];
            FailedResults = [failure];
            IsAllFailure = true;
        }

        public static BatchRequestResult<T> Return(IReadOnlyList<RequestResult<T>> results) => new BatchRequestResult<T>(results);
        public static BatchRequestResult<T> BatchFailure(ErrorCode errorCode, string? errorMessage = null) => new BatchRequestResult<T>(errorCode, errorMessage);

        public static BatchRequestResult<P> Project<T, P>(BatchRequestResult<T> source, Func<T, P> selector)
        {
            var mapped = source.Results.Select(r =>
            {
                if (!r.IsSuccess)
                    return RequestResult<P>.Failure(r.ErrorCode ?? ErrorCode.InvalidData, r.ErrorMessage);

                var value = selector(r.Value!);
                return value is null
                    ? RequestResult<P>.Failure(ErrorCode.InvalidData, "Value is null")
                    : RequestResult<P>.Success(value);
            }).ToList();

            return BatchRequestResult<P>.Return(mapped);
        }
    }
}
