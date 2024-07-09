using InfoTrackSearchModel.Models;

namespace InfoTrackSearchBlazor.Services;

public class SearchService : ISearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IHttpClientFactory httpClientFactory, ILogger<SearchService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SearchResult> PerformSearchAsync(SearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        try
        {
            var client = _httpClientFactory.CreateClient("SearchAPI");
            var response = await client.PostAsJsonAsync("api/search", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SearchResult>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Search request failed with status code: {StatusCode}. Content: {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Search request failed with status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "An error occurred during the HTTP request.");
            throw new ApplicationException("Search request failed. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            throw new ApplicationException("An unexpected error occurred. Please try again later.", ex);
        }
    }

    public async Task<List<SearchResult>> GetSearchHistoryAsync(string keyword, string url)
    {
        ArgumentNullException.ThrowIfNull(keyword, nameof(keyword));
        ArgumentNullException.ThrowIfNull(url, nameof(url));

        try
        {
            var client = _httpClientFactory.CreateClient("SearchAPI");
            var response = await client.GetFromJsonAsync<List<SearchResult>>(
                $"api/search/history?keyword={Uri.EscapeDataString(keyword)}&url={Uri.EscapeDataString(url)}");

            return response ?? new List<SearchResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching search history.");
            throw new ApplicationException("Failed to load search history. Please try again later.", ex);
        }
    }
}
