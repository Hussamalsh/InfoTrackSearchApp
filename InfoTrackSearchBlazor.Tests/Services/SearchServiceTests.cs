using InfoTrackSearchBlazor.Services;
using InfoTrackSearchModel.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace InfoTrackSearchBlazor.Tests.Services;

[TestFixture]
public class SearchServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<ILogger<SearchService>> _loggerMock;
    private SearchService _searchService;

    [SetUp]
    public void SetUp()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SearchService>>();
        _searchService = new SearchService(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task PerformSearchAsync_ValidResponse_ReturnsSearchResult()
    {
        // Arrange
        var searchRequest = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        var searchResult = new SearchResult { Positions = new List<int> { 1 } };

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(searchResult)
            });

        var client = new HttpClient(httpClientMock.Object) { BaseAddress = new Uri("https://example.com/") };
        _httpClientFactoryMock.Setup(x => x.CreateClient("SearchAPI")).Returns(client);

        // Act
        var result = await _searchService.PerformSearchAsync(searchRequest);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(searchResult.Positions, result.Positions);
    }

    [Test]
    public void PerformSearchAsync_InvalidResponse_ThrowsHttpRequestException()
    {
        // Arrange
        var searchRequest = new SearchRequest { Keyword = "test", Url = "https://example.com" };

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad request")
            });

        var client = new HttpClient(httpClientMock.Object) { BaseAddress = new Uri("https://example.com/") };
        _httpClientFactoryMock.Setup(x => x.CreateClient("SearchAPI")).Returns(client);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _searchService.PerformSearchAsync(searchRequest));
        Assert.That(ex.Message, Is.EqualTo("Search request failed. Please try again later."));
        Assert.That(ex.InnerException, Is.TypeOf<HttpRequestException>());
        Assert.That(ex.InnerException.Message, Is.EqualTo("Search request failed with status code: BadRequest"));
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Search request failed with status code:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task GetSearchHistoryAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var keyword = "test";
        var url = "https://example.com";

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new List<SearchResult>())
            });

        var client = new HttpClient(httpClientMock.Object) { BaseAddress = new Uri("https://example.com/") };
        _httpClientFactoryMock.Setup(x => x.CreateClient("SearchAPI")).Returns(client);

        // Act
        var result = await _searchService.GetSearchHistoryAsync(keyword, url);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetSearchHistoryAsync_Exception_ThrowsHttpRequestException()
    {
        // Arrange
        var keyword = "test";
        var url = "https://example.com";

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = new HttpClient(httpClientMock.Object) { BaseAddress = new Uri("https://example.com/") };
        _httpClientFactoryMock.Setup(x => x.CreateClient("SearchAPI")).Returns(client);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ApplicationException>(async () => await _searchService.GetSearchHistoryAsync(keyword, url));
        Assert.That(ex.Message, Is.EqualTo("Failed to load search history. Please try again later."));
        Assert.That(ex.InnerException, Is.TypeOf<HttpRequestException>());
        Assert.That(ex.InnerException.Message, Is.EqualTo("Network error"));
    }
}
