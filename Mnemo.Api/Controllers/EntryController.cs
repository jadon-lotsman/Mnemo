using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Contracts.Entry;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Entities;
using Mnemo.Infrastructure.Queries;
using Mnemo.Infrastructure.RequestResults;
using Mnemo.Services.EntityManagementServices;
using Mnemo.Shared.Enums;
using System.Security.Claims;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/vocabularies/{guid}/entries")]
    public class EntryController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly GlossaryEntryQueries _entryQueries;
        private readonly EntryManagementService _entryService;


        public EntryController(IMapper mapper, GlossaryEntryQueries entryQueries, EntryManagementService entryService)
        {
            _mapper = mapper;
            _entryQueries = entryQueries;
            _entryService = entryService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


        [HttpGet]
        public async Task<IActionResult> SearchInVocabulary([FromQuery] string query)
        {
            var entries = await _entryQueries.GetByQueryAsync(UserId, query);

            if (entries == null)
                return NotFound();

            var entriesResponse = _mapper.Map<List<EntryResponse>>(entries);
            return Ok(entriesResponse);
        }

        [HttpGet("{startLetter:alpha}-{endLetter:alpha}")]
        public async Task<IActionResult> PageEntries(Guid guid, string startLetter, string endLetter, [FromQuery] int page, int pageSize)
        {
            var result = await _entryService.PageEntriesAsync(UserId, guid, startLetter, endLetter, page, pageSize);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGlossaryEntries(Guid guid, [FromBody] CreateEntryRequest[] requests)
        {
            var result = await _entryService.CreateGlossaryEntriesAsync(UserId, guid, requests);
            return result.ToActionResult<GlossaryEntry, EntryResponse>(_mapper, StatusCodes.Status201Created);
        }

        [HttpPost("/api/entries")]
        public async Task<IActionResult> CreateEntries([FromBody] CreateEntryRequest[] requests)
        {
            var result = await _entryService.CreateEntriesAsync(UserId, requests);
            return result.ToActionResult<GlossaryEntry, EntryResponse>(_mapper, StatusCodes.Status201Created);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchEntry(Guid guid, int id, [FromBody] PatchEntryRequest request)
        {
            var result = await _entryService.PatchEntryAsync(UserId, guid, id, request);
            return result.ToActionResult<GlossaryEntry, EntryResponse>(_mapper);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEntry(Guid guid, int id)
        {
            var result = await _entryService.RemoveLinkAsync(UserId, guid, id);
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }
    }
}
