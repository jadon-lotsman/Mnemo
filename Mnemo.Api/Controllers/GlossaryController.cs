using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mnemo.Contracts.Glossary;
using Mnemo.Contracts.Glossary.Requests;
using Mnemo.Entities;
using Mnemo.Infrastructure.Queries;
using Mnemo.Infrastructure.RequestResults;
using Mnemo.Infrastructure.RequestResults.ErrorCodes;
using Mnemo.Services.EntityManagementServices;
using System.Security.Claims;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/vocabularies")]
    public class GlossaryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly GlossaryQueries _glossaryQueries;
        private readonly GlossaryManagementService _glossaryService;


        public GlossaryController(
            IMapper mapper,
            GlossaryQueries glossaryQueries,
            GlossaryManagementService glossaryService)
        {
            _mapper = mapper;
            _glossaryQueries = glossaryQueries;
            _glossaryService = glossaryService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



        [HttpGet]
        public async Task<IActionResult> GetHeaders([FromQuery] int page, int pageSize)
        {
            var result = await _glossaryService.PageGlossariesAsync(UserId, page, pageSize);
            return result.ToActionResult();
        }

        [HttpGet("{guid}/sectors")]
        public async Task<IActionResult> GetGlossarySectors(Guid guid, [FromQuery] string isDescending)
        {
            var isDescendingBoolean = isDescending == "true" ? true : false;
            var response = await _glossaryService.GetGlossarySectorsAsync(UserId, guid, isDescendingBoolean);
            return Ok(response);
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetGlossaryByGuid([FromRoute] Guid guid)
        {
            var gloss = await _glossaryQueries.GetByIdAsync(UserId, guid);

            if (gloss == null)
                return NotFound();

            var glossResponse = _mapper.Map<GlossaryResponse>(gloss);
            return Ok(glossResponse);
        }


        [HttpPost]
        public async Task<IActionResult> CreateGlossary([FromBody] CreateGlossaryRequest request)
        {
            var result = await _glossaryService.CreateGlossaryAsync(UserId, request);
            return result.ToActionResult<Glossary, GlossaryResponse>(_mapper, StatusCodes.Status201Created);
        }

        [HttpPost("merge")]
        public async Task<IActionResult> ImportGlossary(Guid target, Guid source)
        {
            var result = await _glossaryService.ImportGlossaryAsync(UserId, target, source);
            return result.ToActionResult<Glossary, GlossaryResponse>(_mapper);
        }

        [HttpDelete("{guid}/guid")]
        public async Task<IActionResult> RevokeGlossaryGuid(Guid guid)
        {
            var result = await _glossaryService.RevokeSharingGuidAsync(UserId, guid);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.GlossaryNotFound => NotFound(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            return Ok(new { newGuid = result.Value });
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteGlossary(Guid guid)
        {
            var result = await _glossaryService.RemoveGlossaryAsync(UserId, guid);
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }
    }
}
