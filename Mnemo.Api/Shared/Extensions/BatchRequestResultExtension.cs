using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Controllers.ActionResult;

namespace Mnemo.Shared.Extensions
{
    public static class BatchRequestResultExtension
    {
        public static IActionResult ToActionResult<T>(this BatchRequestResult<T> requestResult, int? successCode = null)
            => new BatchRequestResultActionResult<T, object>(requestResult, successCode);

        public static IActionResult ToActionResult<T, TDto>(this BatchRequestResult<T> requestResult, IMapper mapper, int? successCode = null)
            => new BatchRequestResultActionResult<T, TDto>(requestResult, mapper.Map<T, TDto>, successCode);
    }
}
