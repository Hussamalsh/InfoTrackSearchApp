using InfoTrackSearchData.Context;
using InfoTrackSearchData.Repositories;
using InfoTrackSearchModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfoTrackSearchData.Tests.Repositories
{
    [TestFixture]
    public class SearchResultRepositoryTests
    {
        private Mock<ILogger<SearchResultRepository>> _loggerMock;
        private SearchResultRepository _repository;
        private InfoTrackDbContext _dbContext;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<InfoTrackDbContext>()
                .UseInMemoryDatabase(databaseName: "InfoTrackTestDb")
                .Options;
            _dbContext = new InfoTrackDbContext(options);
            _loggerMock = new Mock<ILogger<SearchResultRepository>>();
            _repository = new SearchResultRepository(_dbContext, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AddAsync_ValidSearchResult_ReturnsAddedResult()
        {
            // Arrange
            var searchResult = new SearchResult { Keyword = "test", Url = "https://example.com", Positions = new List<int> { 1 }, SearchDate = DateTime.UtcNow };

            // Act
            var result = await _repository.AddAsync(searchResult);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(searchResult, result);
        }

        [Test]
        public void GetHistory_ValidRequest_ReturnsSearchHistory()
        {
            // Arrange
            var keyword = "test";
            var url = "https://example.com";
            var searchResult = new SearchResult { Keyword = keyword, Url = url, Positions = new List<int> { 1 }, SearchDate = DateTime.UtcNow };
            _dbContext.SearchResults.Add(searchResult);
            _dbContext.SaveChanges();

            // Act
            var result = _repository.GetHistory(keyword, url);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(keyword, result.First().Keyword);
            Assert.AreEqual(url, result.First().Url);
        }
    }
}
