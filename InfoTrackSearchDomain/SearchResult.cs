namespace InfoTrackSearchModel.Models;

public class SearchResult
{
    public int Id { get; set; }
    public string Keyword { get; set; }
    public string Url { get; set; }
    public List<int> Positions { get; set; }
    public DateTime SearchDate { get; set; }
}