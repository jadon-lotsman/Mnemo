using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Contracts.Dtos.Repetition.Requests;
using Mnemo.Services;
using Mnemo.Services.Queries;
using Mnemo.Shared;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly StateQueries _stateQueries;
        private readonly RepetitionStateService _stateService;

        public ScheduleController(StateQueries stateQueries, RepetitionStateService stateService)
        {
            _stateQueries = stateQueries;
            _stateService = stateService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



        [HttpGet]
        public async Task<IActionResult> GetScheduleDays()
        {
            var days = await _stateService.GetScheduleAsync(UserId);

            return Ok(days);
        }

        [HttpGet("states")]
        public async Task<IActionResult> GetAllRepetitionStates()
        {
            var states = await _stateQueries.GetAllByUserIdAsync(UserId);

            var statesDto = Mapper.MapToDto(states);
            return Ok(statesDto);
        }


        [HttpPost("states/{id:int}/assess")]
        public async Task<IActionResult> SelfAssessmentRepetitionState(int id, [FromBody] QualityAssessmentRequest request)
        {
            var result = await _stateService.SetQualityRepetitionStateAsync(UserId, new Dictionary<int, double> { { id, request.Quality } }, true);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.InvalidData => BadRequest(new { message = result.ErrorMessage }),
                    ErrorCode.TaskNotFound => NotFound(new { message = result.ErrorMessage }),
                    ErrorCode.ActionNotAllowed => BadRequest(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            return NoContent();
        }
    }
}
