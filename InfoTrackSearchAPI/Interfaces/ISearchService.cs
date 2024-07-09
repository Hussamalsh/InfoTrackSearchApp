using InfoTrackSearchModel.Models;

namespace InfoTrackSearchAPI.Interfaces;

public interface ISearchService
{
    Task<SearchResult> GetSearchResultsAsync(SearchRequest request);
    Task<IEnumerable<SearchResult>> GetSearchHistoryAsync(string keyword, string url);

}