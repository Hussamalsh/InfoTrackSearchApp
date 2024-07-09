using InfoTrackSearchAPI.Controllers;
using InfoTrackSearchAPI.Interfaces;
using InfoTrackSearchModel.Models;
using InfoTrackSearchModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfoTrackSearchAPI.Tests.Controllers;

[TestFixture]
public class SearchControllerTests
{
    private Mock<ISearchService> _searchServiceMock;
    private Mock<ILogger<SearchController>> _loggerMock;
    private SearchController _controller;

    [SetUp]
    public void Setup()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _loggerMock = new Mock<ILogger<SearchController>>();
        _controller = new SearchController(_searchServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Post_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Keyword", "Required");

        // Act
        var result = await _controller.Post(new SearchRequest());

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task Post_ValidModel_ReturnsOk()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        var searchResult = new SearchResult { Positions = new List<int> { 1, 2, 3 } };
        _searchServiceMock.Setup(s => s.GetSearchResultsAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await _controller.Post(request) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(searchResult, result.Value);
    }

    [Test]
    public async Task Post_ApplicationException_ReturnsServerError()
    {
        // Arrange
        var request = new SearchRequest { Keyword = "test", Url = "https://example.com" };
        _searchServiceMock.Setup(s => s.GetSearchResultsAsync(request)).ThrowsAsync(new ApplicationException());

        // Act
        var result = await _controller.Post(request) as ObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.StatusCode);
        Assert.AreEqual("An error occurred while processing your request. Please try again later.", result.Value);
    }

    [Test]
    public async Task GetHistory_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Keyword", "Required");

        // Act
        var result = await _controller.GetHistory(new SearchHistoryRequest());

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task GetHistory_ValidModel_ReturnsOk()
    {
        // Arrange
        var request = new SearchHistoryRequest { Keyword = "test", Url = "https://example.com" };
        var searchHistory = new List<SearchResult>
            {
                new SearchResult { Keyword = "test", Url = "https://example.com", Positions = new List<int> { 1 }, SearchDate = DateTime.Now }
            };
        _searchServiceMock.Setup(s => s.GetSearchHistoryAsync(request.Keyword, request.Url)).ReturnsAsync(searchHistory);

        // Act
        var result = await _controller.GetHistory(request) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(searchHistory, result.Value);
    }

    [Test]
    public async Task GetHistory_Exception_ReturnsServerError()
    {
        // Arrange
        var request = new SearchHistoryRequest { Keyword = "test", Url = "https://example.com" };
        _searchServiceMock.Setup(s => s.GetSearchHistoryAsync(request.Keyword, request.Url)).ThrowsAsync(new Exception());

        // Act
        var result = await _controller.GetHistory(request) as ObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.StatusCode);
        Assert.AreEqual("An error occurred while fetching search history. Please try again later.", result.Value);
    }
}