using Microsoft.AspNetCore.Http.HttpResults;
using Mnemo.Infrastructure.ErrorCodes;

namespace Mnemo.Infrastructure.RequestResults
{
    public class BatchRequestResult<T>
    {
        public bool HasAnySuccess { get; }

        public IReadOnlyList<T> Values { get; }
        public IReadOnlyList<RequestResult<T>> Results { get; }
        public IReadOnlyList<RequestResult<T>> Errors { get; }


        public BatchRequestResult(IReadOnlyList<RequestResult<T>> results)
        {
            Results = results;
            Values = results.Where(r => r.IsSuccess).Select(r => r.Value!).ToList();
            Errors = results.Where(r => !r.IsSuccess).ToList();
            HasAnySuccess = Values.Count != 0;
        }

        public BatchRequestResult(ErrorCode errorCode, string? errorMessage)
        {
            Results = [RequestResult<T>.Failure(errorCode, errorMessage)];
            Values = [];
            Errors = Results;
            HasAnySuccess = false;
        }

        public static BatchRequestResult<T> Return(IReadOnlyList<RequestResult<T>> results) => new BatchRequestResult<T>(results);
        public static BatchRequestResult<T> BatchFailure(ErrorCode errorCode, string? errorMessage = null) => new BatchRequestResult<T>(errorCode, errorMessage);
        public static BatchRequestResult<T> Concat(params BatchRequestResult<T>[] batches) => new BatchRequestResult<T>(batches.SelectMany(b => b.Results).ToList());
        public static BatchRequestResult<T> ProjectErrors<V>(BatchRequestResult<V> batch) => new BatchRequestResult<T>(batch.Errors.Select(e => RequestResult<T>.Failure(e.ErrorCode ?? ErrorCode.InvalidData, e.ErrorMessage)).ToList());
    }
}
