using Microsoft.AspNetCore.Mvc;

using InfoTrackSearchAPI.Interfaces;
using InfoTrackSearchModel.Models;
using InfoTrackSearchModel;

namespace InfoTrackSearchAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches for the specified keyword and URL in Google.
    /// </summary>
    /// <param name="request">The search request containing the keyword and URL.</param>
    /// <returns>The positions of the URL in the search results.</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid search request received. ModelState: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _searchService.GetSearchResultsAsync(request);
            return Ok(result);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Application error occurred while processing the search request.");
            return StatusCode(500, "An error occurred while processing your request. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Retrieves the search history for the specified keyword and URL.
    /// </summary>
    /// <param name="request">The search history request containing the keyword and URL.</param>
    /// <returns>The search history for the specified keyword and URL.</returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] SearchHistoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid search history request received. ModelState: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            var history = await _searchService.GetSearchHistoryAsync(request.Keyword, request.Url);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching search history.");
            return StatusCode(500, "An error occurred while fetching search history. Please try again later.");
        }
    }
}