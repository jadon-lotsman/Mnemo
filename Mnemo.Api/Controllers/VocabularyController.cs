using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mnemo.Contracts.Vocabulary;
using Mnemo.Contracts.Vocabulary.Requests;
using Mnemo.Data.Queries;
using Mnemo.Services.VocabularyService;
using Mnemo.Shared.Enums;
using System.Security.Claims;

namespace Mnemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/entries")]
    public class VocabularyController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly VocabularyQueries _vocabularyQueries;
        private readonly VocabularyManagementService _vocabularyService;


        public VocabularyController(IMapper mapper, VocabularyQueries vocabularyQueries, VocabularyManagementService vocabularyService)
        {
            _mapper = mapper;
            _vocabularyQueries = vocabularyQueries;
            _vocabularyService = vocabularyService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



        [HttpGet]
        public async Task<IActionResult> GetVocabularyPage([FromQuery] string startWord, string endWord, int page, int pageSize)
        {
            var response = await _vocabularyService.GetVocabularyPageAsync(UserId, startWord, endWord, page, pageSize);

            return Ok(response);
        }

        [HttpGet("sectors")]
        public async Task<IActionResult> GetVocabularySectors([FromQuery] string isDescending)
        {
            var isDescendingBoolean = isDescending == "true" ? true : false;
            var response = await _vocabularyService.GetVocabularySectorsAsync(UserId, isDescendingBoolean);

            return Ok(response);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetVocabularyStatistics()
        {
            var response = await _vocabularyService.GetVocabularyStatisticsAsync(UserId);

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            var entry = await _vocabularyQueries.GetByIdAsync(UserId, id);

            if (entry == null)
                return NotFound();

            var entryRespose = _mapper.Map<EntryResponse>(entry);
            return Ok(entryRespose);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchInVocabularyByQuery([FromQuery] string query)
        {
            var entries = await _vocabularyQueries.GetByQueryAsync(UserId, query);

            if (entries == null)
                return NotFound();

            var entriesResponse = _mapper.Map<List<EntryResponse>>(entries);
            return Ok(entriesResponse);
        }


        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] CreateEntryRequest request)
        {
            var result = await _vocabularyService.CreateEntryAsync(UserId, request);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.InvalidData => BadRequest(new { message = result.ErrorMessage }),
                    ErrorCode.UserNotFound => NotFound(new { message = result.ErrorMessage }),
                    ErrorCode.DuplicateEntry => Conflict(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            var entry = result.Value;
            var entryRespose = _mapper.Map<EntryResponse>(entry);
            return Ok(entryRespose);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchEntry(int id, [FromBody] PatchEntryRequest request)
        {
            var result = await _vocabularyService.PatchEntryAsync(UserId, id, request);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.InvalidData => BadRequest(new { message = result.ErrorMessage }),
                    ErrorCode.EntryNotFound => NotFound(new { message = result.ErrorMessage }),
                    ErrorCode.DuplicateEntry => Conflict(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            var entry = result.Value;
            var entryRespose = _mapper.Map<EntryResponse>(entry);
            return Ok(entryRespose);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var result = await _vocabularyService.RemoveEntryByIdAsync(UserId, id);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.EntryNotFound => NotFound(new { message = result.ErrorMessage }),
                    _ => StatusCode(500, new { message = result.ErrorMessage })
                };
            }

            return NoContent();
        }
    }
}
