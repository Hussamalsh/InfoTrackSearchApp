namespace InfoTrackSearchAPI.Interfaces;

public interface IHtmlParser
{
    Task<List<int>> ParsePositionsAsync(string html, string url);
}