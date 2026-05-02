using FastTelecom.Application.Services;
using FastTelecom.Domain.Interfaces;
using FastTelecom.Domain.Models;
using NSubstitute;

namespace FastTelecom.Application.Tests
{
    public class BundleServiceTests
    {
        private readonly ITarasClient _tarasClient = Substitute.For<ITarasClient>();
        private readonly IBundleClient _bundleClient = Substitute.For<IBundleClient>();
        private readonly BundleService _sut;
        private readonly SessionStore _session = new();

        public BundleServiceTests()
        {
            _sut = new BundleService(_bundleClient, _tarasClient, _session);
        }


        [Fact]
        public async Task GetBundlesAsync_NoSession_ReturnsFail()
        {
            var result = await _sut.GetBundlesAsync();

            Assert.False(result.Success);
            Assert.Equal("Session expired. Please log in again.", result.Error);
        }

        [Fact]
        public async Task GetBundlesAsync_ValidSession_ReturnsBundles()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _bundleClient
                .GetBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new BundlesApiResponse
                {
                    Basic = 1,
                    Bundles = new[]
                    {
                        new Bundle { Id = 1, Name = "Basic Plan", Price = 9.99m, Vol = 10, IsEnable = 1 }
                    }
                });

            var result = await _sut.GetBundlesAsync();

            Assert.True(result.Success);
            Assert.Single(result.Bundles);
            Assert.Equal("Basic Plan", result.Bundles[0].Name);
            Assert.True(result.Bundles[0].IsAvailable);
        }

        [Fact]
        public async Task GetBundlesAsync_BundleDisabled_IsAvailableFalse()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _bundleClient
                .GetBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new BundlesApiResponse
                {
                    Basic = 1,
                    Bundles = new[]
                    {
                        new Bundle { Id = 2, Name = "Disabled Plan", Price = 4.99m, Vol = 5, IsEnable = 0 }
                    }
                });

            var result = await _sut.GetBundlesAsync();

            Assert.True(result.Success);
            Assert.False(result.Bundles[0].IsAvailable);
        }

        [Fact]
        public async Task GetBundlesAsync_NullResponse_ReturnsFail()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _bundleClient
                .GetBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns((BundlesApiResponse?)null);

            var result = await _sut.GetBundlesAsync();

            Assert.False(result.Success);
            Assert.Equal("Failed to retrieve bundles from server.", result.Error);
        }

        [Fact]
        public async Task PurchaseBundleAsync_NoSession_ReturnsFail()
        {
            var result = await _sut.PurchaseBundleAsync(1, 1);

            Assert.False(result.Success);
            Assert.Equal("Session expired. Please log in again.", result.Error);
        }

        [Fact]
        public async Task PurchaseBundleAsync_Code200_ReturnsSuccess()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _bundleClient
                .PurchaseBundleAsync("testuser", "testpass", Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(new PurchaseApiResponse
                {
                    Success = true,
                    Item = new PurchaseItemResult { Code = 200, Msg = null }
                });

            var result = await _sut.PurchaseBundleAsync(1, 1);

            Assert.True(result.Success);
            Assert.Equal("Bundle purchased successfully!", result.Message);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task PurchaseBundleAsync_CodeNot200_ReturnsFail()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _bundleClient
                .PurchaseBundleAsync("testuser", "testpass", Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(new PurchaseApiResponse
                {
                    Success = true,
                    Item = new PurchaseItemResult { Code = 500, Msg = "Insufficient balance." }
                });

            var result = await _sut.PurchaseBundleAsync(1, 1);

            Assert.False(result.Success);
            Assert.Equal("Insufficient balance.", result.Error);
        }

        [Fact]
        public async Task PurchaseBundleAsync_ApiReturnsFail_ReturnsFail()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _bundleClient
                .PurchaseBundleAsync("testuser", "testpass", Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
                .Returns(new PurchaseApiResponse
                {
                    Success = false,
                    Error = "Service unavailable."
                });

            var result = await _sut.PurchaseBundleAsync(1, 1);

            Assert.False(result.Success);
            Assert.Equal("Service unavailable.", result.Error);
        }

        [Fact]
        public async Task GetActiveBundlesAsync_NoSession_ReturnsFail()
        {
            var result = await _sut.GetActiveBundlesAsync();

            Assert.False(result.Success);
            Assert.Equal("Session expired. Please log in again.", result.Error);
        }

        [Fact]
        public async Task GetActiveBundlesAsync_UnlimitedBundle_MapsCorrectly()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _tarasClient
                .GetActiveBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new[]
                {
                    new ActiveBundle
                    {
                        ProductID = "1",
                        ProductName = "Unlimited Plan",
                        MaxServiceUsage = 0,
                        FreeVolume = 0,
                        AccumulateInfo = new ActiveBundleAccumulateInfo { MonthAccuVolume = 0 }
                    }
                });

            var result = await _sut.GetActiveBundlesAsync();

            Assert.True(result.Success);
            Assert.Single(result.Bundles);
            Assert.True(result.Bundles[0].IsUnlimited);
            Assert.Equal("Unlimited", result.Bundles[0].TotalDisplay);
        }

        [Fact]
        public async Task GetActiveBundlesAsync_VolumeOver1024MB_DisplaysAsGb()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _tarasClient
                .GetActiveBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new[]
                {
                    new ActiveBundle
                    {
                        ProductID = "2",
                        ProductName = "10 GB Plan",
                        MaxServiceUsage = 10240,
                        FreeVolume = 10240 * 1024L,
                        AccumulateInfo = new ActiveBundleAccumulateInfo { MonthAccuVolume = 0 }
                    }
                });

            var result = await _sut.GetActiveBundlesAsync();

            Assert.Contains("GB", result.Bundles[0].TotalDisplay);
        }

        [Fact]
        public async Task GetActiveBundlesAsync_ExpiringWithin7Days_IsExpiringSoonTrue()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            var expiry = DateTime.Now.AddDays(3).ToString("yyyyMMddHHmmss");

            _tarasClient
                .GetActiveBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new[]
                {
                    new ActiveBundle
                    {
                        ProductID = "3",
                        ProductName = "Expiring Plan",
                        MaxServiceUsage = 1024,
                        FreeVolume = 1024 * 1024L,
                        ExpTime = expiry,
                        AccumulateInfo = new ActiveBundleAccumulateInfo { MonthAccuVolume = 0 }
                    }
                });

            var result = await _sut.GetActiveBundlesAsync();

            Assert.True(result.Bundles[0].IsExpiringSoon);
        }

        [Fact]
        public async Task GetActiveBundlesAsync_InvalidDateString_ExpiryDateIsDash()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _tarasClient
                .GetActiveBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new[]
                {
                    new ActiveBundle
                    {
                        ProductID = "4",
                        ProductName = "Bad Date Plan",
                        MaxServiceUsage = 1024,
                        FreeVolume = 1024 * 1024L,
                        ExpTime = "invalid",
                        AccumulateInfo = new ActiveBundleAccumulateInfo { MonthAccuVolume = 0 }
                    }
                });

            var result = await _sut.GetActiveBundlesAsync();

            Assert.Equal("-", result.Bundles[0].ExpiryDate);
        }

        [Fact]
        public async Task GetActiveBundlesAsync_OnlineSession_IsOnlineTrue()
        {
            _session.Username = "testuser";
            _session.Password = "testpass";

            _tarasClient
                .GetActiveBundlesAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new[]
                {
                    new ActiveBundle
                    {
                        ProductID = "5",
                        ProductName = "Active Plan",
                        MaxServiceUsage = 1024,
                        FreeVolume = 1024 * 1024L,
                        OnlineSessionNum = 1,
                        AccumulateInfo = new ActiveBundleAccumulateInfo { MonthAccuVolume = 0 }
                    }
                });

            var result = await _sut.GetActiveBundlesAsync();

            Assert.True(result.Bundles[0].IsOnline);
        }
    }
}
