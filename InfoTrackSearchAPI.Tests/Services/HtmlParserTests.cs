using InfoTrackSearchAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfoTrackSearchAPI.Tests.Services;

[TestFixture]
public class HtmlParserTests
{
    private Mock<ILogger<HtmlParser>> _loggerMock;
    private HtmlParser _htmlParser;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<HtmlParser>>();
        _htmlParser = new HtmlParser(_loggerMock.Object);
    }

    [Test]
    public async Task ParsePositions_ValidHtmlAndUrl_ReturnsPositions()
    {
        // Arrange
        var htmlContent = "<html><body><a href=\"/url?q=https://example.com\">Link</a></body></html>";
        var targetUrl = "https://example.com";

        // Act
        var positions = await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);

        // Assert
        Assert.IsNotNull(positions);
        Assert.AreEqual(1, positions.Count);
        Assert.AreEqual(1, positions[0]);
    }

    [Test]
    public async Task ParsePositions_ValidHtmlAndUrl_ReturnsCorrectPositions()
    {
        // Arrange
        var htmlContent = @"<!doctype html>
                            <html lang=""en-GB"">
                            <body>
                                <a href=""/url?q=https://example.se/&amp;sa=U&amp;ved=2ahUKEwinsOCOpJmHAxWuV6QEHZ_TAeMQFnoECAEQAg&amp;usg=AOvVaw32AoFuuGhqbCMdIf_GlD9Z"">Link1</a>
                                <a href=""/url?q=https://uk.linkedin.com/in/hussam-alshammari-27bbbbb8&amp;sa=U&amp;ved=2ahUKEwinsOCOpJmHAxWuV6QEHZ_TAeMQFnoECAMQAg&amp;usg=AOvVaw3JWrDXgnYclF5OMA9fha0y"">Link2</a>
                                <a href=""/url?q=https://www.udemy.com/user/hussam-alshammari-2/&amp;sa=U&amp;ved=2ahUKEwinsOCOpJmHAxWuV6QEHZ_TAeMQFnoECAQQAg&amp;usg=AOvVaw21dDR1Lm7ncJMCATOeMR3Q"">Link3</a>
                                <a href=""/url?q=https://example.se/Hussam_CV.pdf&amp;sa=U&amp;ved=2ahUKEwinsOCOpJmHAxWuV6QEHZ_TAeMQFnoECAIQAg&amp;usg=AOvVaw0JsOGf_71Lv0-ZuLmqbljr"">Link4</a>
                            </body>
                            </html>";
        var targetUrl = "https://example.se/";

        // Act
        var positions = await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);

        // Assert
        Assert.IsNotNull(positions);
        Assert.AreEqual(2, positions.Count);
        Assert.AreEqual(1, positions[0]);
        Assert.AreEqual(4, positions[1]);
    }


    [Test]
    public void ParsePositions_EmptyHtmlContent_ThrowsArgumentException()
    {
        // Arrange
        var htmlContent = "";
        var targetUrl = "https://example.com";

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);
        });
        Assert.That(ex.ParamName, Is.EqualTo("htmlContent"));
    }



    [Test]
    public void ParsePositions_NullHtmlContent_ThrowsArgumentException()
    {
        // Arrange
        string htmlContent = null;
        var targetUrl = "https://example.com";

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);
        });
        Assert.That(ex.ParamName, Is.EqualTo("htmlContent"));
    }

    [Test]
    public void ParsePositions_EmptyTargetUrl_ThrowsArgumentException()
    {
        // Arrange
        var htmlContent = "<html><body><a href=\"/url?q=https://example.com\">Link</a></body></html>";
        var targetUrl = "";

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);
        });
        Assert.That(ex.ParamName, Is.EqualTo("targetUrl"));
    }

    [Test]
    public void ParsePositions_NullTargetUrl_ThrowsArgumentException()
    {
        // Arrange
        var htmlContent = "<html><body><a href=\"/url?q=https://example.com\">Link</a></body></html>";
        string targetUrl = null;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl));
        Assert.That(ex.ParamName, Is.EqualTo("targetUrl"));
    }

    [Test]
    public async Task ParsePositions_NoOccurrences_ReturnsEmptyList()
    {
        // Arrange
        var htmlContent = "<html><body><a href=\"/url?q=https://example.com\">Link</a></body></html>";
        var targetUrl = "https://nonexistent.com";

        // Act
        var positions = await _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);

        // Assert
        Assert.IsNotNull(positions);
        Assert.IsEmpty(positions);
    }

    [Test]
    public async Task ParsePositions_LoggerLogsInformation()
    {
        // Arrange
        var htmlContent = "<html><body><a href=\"/url?q=https://example.com\">Link</a></body></html>";
        var targetUrl = "https://example.com";

        // Act
        var positions = _htmlParser.ParsePositionsAsync(htmlContent, targetUrl);

        // Assert
        _loggerMock.Verify(logger => logger.Log(
            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 occurrences")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
