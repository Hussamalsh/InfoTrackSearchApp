using InfoTrackSearchAPI.Interfaces;
using System.Text.RegularExpressions;

namespace InfoTrackSearchAPI.Services;

public class HtmlParser : IHtmlParser
{
    private static readonly Regex LinkRegex = new(@"<a\s+href=""\/url\?q=(http[s]?:\/\/(www\.)?[^&]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ILogger<HtmlParser> _logger;

    public HtmlParser(ILogger<HtmlParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<int>> ParsePositionsAsync(string htmlContent, string targetUrl)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException("HTML content cannot be null or empty.", nameof(htmlContent));
        }

        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            throw new ArgumentException("Target URL cannot be null or empty.", nameof(targetUrl));
        }

        var positions = new List<int>();

        try
        {
            var matches = LinkRegex.Matches(htmlContent);

            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Groups[1].Value.Contains(targetUrl, StringComparison.OrdinalIgnoreCase))
                {
                    positions.Add(i + 1);
                }
            }

            _logger.LogInformation($"Found {positions.Count} occurrences of the target URL in the provided HTML content.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while parsing the HTML content.");
            throw;
        }

        return await Task.FromResult(positions); // Use Task.FromResult to wrap the result in a Task
    }
}