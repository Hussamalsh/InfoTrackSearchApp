using InfoTrackSearchModel.Models;

namespace InfoTrackSearchData.Interfaces;

public interface ISearchResultRepository
{
    Task<SearchResult> AddAsync(SearchResult searchResult);
    IQueryable<SearchResult> GetHistory(string keyword, string url);
}