using InfoTrackSearchAPI.Services;
using InfoTrackSearchAPI.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace InfoTrackSearchAPI.Tests.Services
{
    [TestFixture]
    public class CacheServiceTests
    {
        private Mock<IMemoryCache> _memoryCacheMock;
        private Mock<ILogger<CacheService>> _loggerMock;
        private Mock<IOptions<CacheSettings>> _cacheSettingsMock;
        private CacheService _cacheService;

        [SetUp]
        public void SetUp()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<CacheService>>();
            _cacheSettingsMock = new Mock<IOptions<CacheSettings>>();

            var cacheSettings = new CacheSettings { ExpirationMinutes = 10 };
            _cacheSettingsMock.Setup(s => s.Value).Returns(cacheSettings);

            _cacheService = new CacheService(
                _memoryCacheMock.Object,
                _loggerMock.Object,
                _cacheSettingsMock.Object);
        }


        [Test]
        public void GetOrCreateAsync_NullOrEmptyKey_ThrowsArgumentException()
        {
            // Arrange
            var key = "";
            Func<Task<string>> createItem = () => Task.FromResult("newValue");

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _cacheService.GetOrCreateAsync(key, createItem));
            Assert.That(ex.ParamName, Is.EqualTo("key"));
        }

        [Test]
        public async Task GetOrCreateAsync_CreateItemThrowsException_LogsErrorAndThrows()
        {
            // Arrange
            var key = "testKey";
            object cacheValue = null;
            _memoryCacheMock.Setup(mc => mc.TryGetValue(key, out cacheValue)).Returns(false);

            var exception = new Exception("Error creating item");
            Func<Task<string>> createItem = () => throw exception;

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _cacheService.GetOrCreateAsync(key, createItem));
            Assert.AreEqual(exception, ex);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error creating cache entry for key: {key}")),
                exception,
                It.Is<Func<It.IsAnyType, Exception, string>>((o, t) => true)), Times.Once);
        }
    }
}
