using InfoTrackSearchAPI.Interfaces;
using InfoTrackSearchAPI.Services;
using InfoTrackSearchAPI.Settings;
using InfoTrackSearchAPI.Tests.Helpers;
using InfoTrackSearchData.Interfaces;
using InfoTrackSearchModel.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace InfoTrackSearchAPI.Tests;

[TestFixture]
public class SearchServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<ILogger<SearchService>> _loggerMock;
    private Mock<IHtmlParser> _htmlParserMock;
    private Mock<IOptions<GoogleSearchSettings>> _settingsMock;
    private Mock<ICacheService> _cacheServiceMock;
    private Mock<ISearchResultRepository> _searchResultRepositoryMock;
    private SearchService _searchService;

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SearchService>>();
        _htmlParserMock = new Mock<IHtmlParser>();
        _settingsMock = new Mock<IOptions<GoogleSearchSettings>>();
        _cacheServiceMock = new Mock<ICacheService>();
        _searchResultRepositoryMock = new Mock<ISearchResultRepository>();

        var settings = new GoogleSearchSettings { BaseUrl = "https://www.google.co.uk/search?num=100&q=" };
        _settingsMock.Setup(s => s.Value).Returns(settings);

        _searchService = new SearchService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _htmlParserMock.Object,
            _settingsMock.Object,
            _cacheServiceMock.Object,
            _searchResultRepositoryMock.Object);
    }

    #region GetSearchResultsAsync
    [Test]
    public async Task GetSearchResultsAsync_ValidRequest_ReturnsSearchResult()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        var searchResult = new SearchResult { Keyword = "test", Url = "https://example.com", Positions = new List<int> { 1 }, SearchDate = DateTime.UtcNow };

        var httpClient = new HttpClient(new FakeHttpMessageHandler("mock response"));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _htmlParserMock.Setup(p => p.ParsePositionsAsync(It.IsAny<string>(), request.Url)).ReturnsAsync(new List<int> { 1 });
        _searchResultRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SearchResult>())).ReturnsAsync(searchResult);
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>())).ReturnsAsync(searchResult);

        // Act
        var result = await _searchService.GetSearchResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(request.Keyword, result.Keyword);
        Assert.AreEqual(request.Url, result.Url);
        Assert.Contains(1, result.Positions);
    }

    [Test]
    public async Task GetSearchResultsAsync_CacheHit_ReturnsCachedResult()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        var cachedResult = new SearchResult { Keyword = "test", Url = "https://example.com", Positions = new List<int> { 1 }, SearchDate = DateTime.UtcNow };

        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>()))
                         .ReturnsAsync(cachedResult);

        // Act
        var result = await _searchService.GetSearchResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(cachedResult, result);
        _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetSearchResultsAsync_NoPositionsFound_AddsZeroPosition()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        var httpClient = new HttpClient(new FakeHttpMessageHandler("mock response"));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _htmlParserMock.Setup(p => p.ParsePositionsAsync(It.IsAny<string>(), request.Url)).ReturnsAsync(new List<int>());
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>()))
                         .Returns((string key, Func<Task<SearchResult>> factory) => factory());

        // Act
        var result = await _searchService.GetSearchResultsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.Contains(0, result.Positions);
    }

    [Test]
    public void GetSearchResultsAsync_RepositoryAddFails_ThrowsApplicationException()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        var httpClient = new HttpClient(new FakeHttpMessageHandler("mock response"));
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _htmlParserMock.Setup(p => p.ParsePositionsAsync(It.IsAny<string>(), request.Url)).ReturnsAsync(new List<int> { 1 });
        _searchResultRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SearchResult>())).ThrowsAsync(new Exception("Database error"));
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>()))
                         .Returns((string key, Func<Task<SearchResult>> factory) => factory());

        // Act & Assert
        var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
            await _searchService.GetSearchResultsAsync(request));

        Assert.That(exception.Message, Is.EqualTo("There was a problem fetching the search results. Please try again later."));
        _searchResultRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SearchResult>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("An unexpected error occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void GetSearchResultsAsync_CacheServiceThrowsException_ThrowsApplicationException()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>()))
                         .ThrowsAsync(new Exception("Cache error"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _searchService.GetSearchResultsAsync(request));
        Assert.That(exception.Message, Is.EqualTo("There was a problem fetching the search results. Please try again later."));
    }

    [Test]
    public void GetSearchResultsAsync_HttpRequestException_ThrowsApplicationException()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                              .Returns(new HttpClient(new FakeHttpMessageHandler("mock response", new HttpRequestException())));

        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>()))
                         .Returns((string key, Func<Task<SearchResult>> factory) => factory());

        // Act & Assert
        var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _searchService.GetSearchResultsAsync(request));

        Assert.That(exception.Message, Is.EqualTo("There was a problem fetching the search results. Please try again later."));
    }

    [Test]
    public void GetSearchResultsAsync_GeneralException_ThrowsApplicationException()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                              .Returns(new HttpClient(new FakeHttpMessageHandler("mock response", new Exception("Test exception"))));

        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<SearchResult>>>()))
                         .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<ApplicationException>(async () => await _searchService.GetSearchResultsAsync(request));

        Assert.That(exception.Message, Is.EqualTo("There was a problem fetching the search results. Please try again later."));
    }

    #endregion

    #region GetSearchHistoryAsync

    [Test]
    public void GetSearchHistoryAsync_Exception_ThrowsException()
    {
        // Arrange
        var keyword = "test";
        var url = "https://example.com";

        // Mock GetHistory to throw an exception
        _searchResultRepositoryMock.Setup(r => r.GetHistory(keyword, url)).Callback(() => throw new Exception());

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _searchService.GetSearchHistoryAsync(keyword, url));
    }

    [Test]
    public async Task GetSearchHistoryAsync_ValidRequest_ReturnsHistory()
    {
        // Arrange
        var keyword = "test";
        var url = "https://example.com";
        var history = new List<SearchResult>
        {
            new SearchResult { Keyword = keyword, Url = url, Positions = new List<int> { 1 }, SearchDate = DateTime.UtcNow }
        };

        _searchResultRepositoryMock.Setup(r => r.GetHistory(keyword, url)).Returns(history.AsAsyncQueryable());

        // Act
        var result = await _searchService.GetSearchHistoryAsync(keyword, url);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(keyword, result.First().Keyword);
        Assert.AreEqual(url, result.First().Url);
    }
    #endregion
    
}
