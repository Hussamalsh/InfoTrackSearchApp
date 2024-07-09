using InfoTrackSearchAPI.Interfaces;
using InfoTrackSearchAPI.Settings;
using InfoTrackSearchData.Interfaces;
using InfoTrackSearchModel.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InfoTrackSearchAPI.Services;

public class SearchService : ISearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SearchService> _logger;
    private readonly IHtmlParser _htmlParser;
    private readonly GoogleSearchSettings _appSettings;
    private readonly ICacheService _cacheService;
    private readonly ISearchResultRepository _searchResultRepository;

    public SearchService(
        IHttpClientFactory httpClientFactory,
        ILogger<SearchService> logger,
        IHtmlParser htmlParser,
        IOptions<GoogleSearchSettings> settings,
        ICacheService cacheService,
        ISearchResultRepository searchResultRepository)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _htmlParser = htmlParser ?? throw new ArgumentNullException(nameof(htmlParser));
        _appSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _searchResultRepository = searchResultRepository ?? throw new ArgumentNullException(nameof(searchResultRepository));
    }

    public async Task<SearchResult> GetSearchResultsAsync(SearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var cacheKey = $"{request.Keyword}_{request.Url}";
        try 
        {
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var result = new SearchResult
                {
                    Keyword = request.Keyword,
                    Url = request.Url,
                    Positions = new List<int>(),
                    SearchDate = DateTime.UtcNow
                };

                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.GetStringAsync($"{_appSettings.BaseUrl}{Uri.EscapeDataString(request.Keyword)}");
                    result.Positions = await _htmlParser.ParsePositionsAsync(response, request.Url);

                    if (!result.Positions.Any())
                    {
                        result.Positions.Add(0);
                    }

                    await _searchResultRepository.AddAsync(result);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching search results.");
                    throw new ApplicationException("There was a problem fetching the search results. Please try again later.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred.");
                    throw new ApplicationException("An unexpected error occurred. Please try again later.", ex);
                }

                return result;
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching or creating cached search results.");
            throw new ApplicationException("There was a problem fetching the search results. Please try again later.", ex);
        }
    }

    public async Task<IEnumerable<SearchResult>> GetSearchHistoryAsync(string keyword, string url)
    {
        ArgumentNullException.ThrowIfNull(keyword, nameof(keyword));
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        try
        {
            return await _searchResultRepository.GetHistory(keyword, url).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching search history.");
            throw;
        }
    }
}