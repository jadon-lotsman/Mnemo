using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Controllers.ActionResults;

namespace Mnemo.Infrastructure.RequestResults
{
    public static class RequestResultExtension
    {
        public static IActionResult ToActionResult<T>(this RequestResult<T> requestResult, int? successCode = null)
            => new RequestResultActionResult<T, object>(requestResult, successCode);

        public static IActionResult ToActionResult<T, TDto>(this RequestResult<T> requestResult, IMapper mapper, int? successCode = null)
            => new RequestResultActionResult<T, TDto>(requestResult, mapper.Map<T, TDto>, successCode);
    }
}
