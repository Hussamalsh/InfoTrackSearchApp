using InfoTrackSearchData.Context;
using InfoTrackSearchData.Interfaces;
using InfoTrackSearchModel.Models;
using Microsoft.Extensions.Logging;

namespace InfoTrackSearchData.Repositories;

public class SearchResultRepository : ISearchResultRepository
{
    private readonly InfoTrackDbContext _context;
    private readonly ILogger<SearchResultRepository> _logger;

    public SearchResultRepository(InfoTrackDbContext context, ILogger<SearchResultRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SearchResult> AddAsync(SearchResult searchResult)
    {
        if (searchResult == null)
        {
            throw new ArgumentNullException(nameof(searchResult));
        }

        try
        {
            await _context.SearchResults.AddAsync(searchResult);
            await _context.SaveChangesAsync();
            return searchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a search result.");
            throw new ApplicationException("An error occurred while adding a search result. Please try again later.", ex);
        }
    }

    public IQueryable<SearchResult> GetHistory(string keyword, string url)
    {
        if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Keyword and URL must be provided.");
        }

        try
        {
            return _context.SearchResults
                .Where(r => r.Keyword == keyword && r.Url == url)
                .OrderByDescending(r => r.SearchDate)
                .AsQueryable();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving search history.");
            throw new ApplicationException("An error occurred while retrieving search history. Please try again later.", ex);
        }
    }
}