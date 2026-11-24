using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mnemo.Infrastructure.ErrorCodes;
using Mnemo.Infrastructure.RequestResults;

namespace Mnemo.Controllers.ActionResults
{
    public class RequestResultActionResult<T, TDto> : IActionResult
    {
        private readonly RequestResult<T> _result;
        private readonly Func<T, TDto>? _mapFunc;
        private readonly int _successStatusCode = StatusCodes.Status200OK;


        public RequestResultActionResult(RequestResult<T> result, int? successCode = null)
        {
            _result = result;
            _successStatusCode = successCode ?? _successStatusCode;
        }

        public RequestResultActionResult(RequestResult<T> result, Func<T, TDto> mapFunc, int? successCode = null)
        {
            _result = result;
            _mapFunc = mapFunc;
            _successStatusCode = successCode ?? _successStatusCode;
        }


        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            var jsonOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;

            if (_result.IsSuccess)
            {
                response.StatusCode = _successStatusCode;

                if (!IsStatusCodeWithoutBody(_successStatusCode))
                {
                    object value = _mapFunc != null
                        ? _mapFunc(_result.Value!)!
                        : _result.Value!;

                    await response.WriteAsJsonAsync(value, jsonOptions.JsonSerializerOptions);
                }
            }
            else
            {
                response.StatusCode = _result.ErrorCode.ToHttpCode();

                var errorResponse = new { message = _result.ErrorMessage };
                await response.WriteAsJsonAsync(errorResponse, jsonOptions.JsonSerializerOptions);
            }
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
