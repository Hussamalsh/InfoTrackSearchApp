using System.ComponentModel.DataAnnotations;

namespace InfoTrackSearchModel;

public class SearchHistoryRequest
{
    [Required(ErrorMessage = "Keyword is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Keyword must be between 1 and 100 characters.")]
    public string? Keyword { get; set; }

    [Required(ErrorMessage = "URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    public string? Url { get; set; }
}
