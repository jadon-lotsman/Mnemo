using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mnemo.Infrastructure.ErrorCodes;
using Mnemo.Infrastructure.RequestResults;

namespace Mnemo.Controllers.ActionResults
{
    public class BatchRequestResultActionResult<T, TDto> : IActionResult
    {
        private readonly BatchRequestResult<T> _result;
        private readonly Func<T, TDto>? _mapFunc;
        private readonly int _successStatusCode = StatusCodes.Status200OK;


        public BatchRequestResultActionResult(BatchRequestResult<T> result, int? successCode = null)
        {
            _result = result;
            _successStatusCode = successCode ?? _successStatusCode;
        }

        public BatchRequestResultActionResult(BatchRequestResult<T> result, Func<T, TDto>? mapFunc, int? successCode = null)
        {
            _result = result;
            _mapFunc = mapFunc;
            _successStatusCode = successCode ?? _successStatusCode;
        }


        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            var jsonOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;

            if (_result.Results.Count == 0)
            {
                response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            if (_result.Errors.Count == 0)
            {
                response.StatusCode = _successStatusCode;
                if (!IsStatusCodeWithoutBody(_successStatusCode))
                {
                    var values = _mapFunc != null
                        ? _result.Values.Select(_mapFunc).Cast<object>().ToList()
                        : _result.Values.Cast<object>().ToList();

                    await response.WriteAsJsonAsync(values, jsonOptions.JsonSerializerOptions);
                }
                return;
            }

            if (_result.Values.Count == 0)
            {
                var codes = _result.Errors
                    .Select(e => e.ErrorCode)
                    .Distinct()
                    .ToList();

                response.StatusCode = codes.Count == 1
                    ? codes[0].ToHttpCode()
                    : StatusCodes.Status400BadRequest;

                var errorResponse = _result.Errors.Select(e => new { message = e.ErrorMessage! });
                await response.WriteAsJsonAsync(errorResponse, jsonOptions.JsonSerializerOptions);
                return;
            }

            response.StatusCode = StatusCodes.Status207MultiStatus;

            var results = _result.Results
                .Select(r => r.IsSuccess
                ? (object)new
                {
                    value = _mapFunc != null
                    ? (object)_mapFunc(r.Value!)! 
                    : r.Value
                }
                : new
                {
                    message = r.ErrorMessage
                })
                .ToList();

            await response.WriteAsJsonAsync(results, jsonOptions.JsonSerializerOptions);
        }

        private static bool IsStatusCodeWithoutBody(int statusCode)
        {
            return statusCode is >= 100 and <= 199
                or StatusCodes.Status204NoContent
                or StatusCodes.Status205ResetContent
                or StatusCodes.Status304NotModified;
        }
    }
}
