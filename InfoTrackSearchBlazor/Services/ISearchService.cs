using InfoTrackSearchModel.Models;

namespace InfoTrackSearchBlazor.Services;
public interface ISearchService
{
    Task<SearchResult> PerformSearchAsync(SearchRequest request);
    Task<List<SearchResult>> GetSearchHistoryAsync(string keyword, string url);

}
