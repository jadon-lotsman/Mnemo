using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mnemo.Contracts.Dtos.Repetition.Requests;
using Mnemo.Services;
using Mnemo.Services.Queries;
using Mnemo.Shared;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RepetitionController : ControllerBase
    {
        private readonly TaskQueries _taskQueries;
        private readonly RepetitionTaskService _taskService;

        public RepetitionController(TaskQueries sessionQueries, RepetitionTaskService taskService)
        {
            _taskQueries = sessionQueries;
            _taskService = taskService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



        [HttpGet]
        public async Task<IActionResult> ExistsRepetitionStatus()
        {
            var result = await _taskService.ExistsRepetitionAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.UserNotFound => NotFound(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            return Ok(new { inProcess = result.Value });
        }

        [HttpPost]
        public async Task<IActionResult> StartRepetitionSession([FromQuery] string mode)
        {
            var result = await _taskService.StartRepetitionAsync(UserId, mode);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.InvalidData => BadRequest(new { message = result.ErrorMessage }),
                    ErrorCode.UserNotFound => NotFound(new { message = result.ErrorMessage }),
                    ErrorCode.TaskGenerationFailed => UnprocessableEntity(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            return Ok(result.Value);
        }

        [HttpDelete]
        public async Task<IActionResult> FinishRepetitionSession()
        {
            var result = await _taskService.FinishRepetitionAsync(UserId);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.InvalidData => BadRequest(new { message = result.ErrorMessage }),
                    ErrorCode.StateNotFound => NotFound(new { message = result.ErrorMessage }),
                    ErrorCode.RepetitionNotFound => NotFound(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            var resultDto = Mapper.MapToDto(result.Value);
            return Ok(resultDto);
        }



        [HttpGet("tasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskQueries.GetByUserIdQuery(UserId).ToListAsync();

            var tasksDto = Mapper.MapToDto(tasks);
            return Ok(tasksDto);
        }

        [HttpGet("tasks/{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _taskQueries.GetTaskByIdAsync(UserId, id);

            if (task == null)
                return NotFound();

            var tasksDto = Mapper.MapToDto(task);
            return Ok(tasksDto);
        }

        [HttpPost("tasks/{id:int}")]
        public async Task<IActionResult> SubmitTaskAnswer(int id, [FromBody] SubmitTaskAnswerRequest request)
        {
            var result = await _taskService.SubmitRepetitionTaskAnswerAsync(UserId, id, request.UserAnswer, TimeSpan.FromMilliseconds(request.ElapsedTimeMilliseconds));

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.TaskNotFound => NotFound(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            var tasksDto = Mapper.MapToDto(result.Value);
            return Ok(tasksDto);
        }
    }
}
