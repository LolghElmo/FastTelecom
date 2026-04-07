using FastTelecom.Application.DTOs;
using FastTelecom.Application.Services;
using FastTelecom.Domain.Interfaces;
using FastTelecom.Domain.Models;
using NSubstitute;

namespace FastTelecom.Application.Tests
{
    public class AuthenticationServiceTests
    {
        private readonly ITarasClient _tarasClient = Substitute.For<ITarasClient>();
        private readonly SessionStore _session = new();
        private readonly AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            _sut = new AuthenticationService(_tarasClient, _session);
        }

        [Fact]
        public async Task LoginAsync_EmptyUsername_ReturnsFail()
        {
            var request = new LoginRequestDto { Username = "", Password = "pass" };

            var result = await _sut.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal("Username is required.", result.Error);
            await _tarasClient.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task LoginAsync_EmptyPassword_ReturnsFail()
        {
            var request = new LoginRequestDto { Username = "user", Password = "" };
            var result = await _sut.LoginAsync(request);
            Assert.False(result.Success);
            Assert.Equal("Password is required.", result.Error);
            await _tarasClient.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
        {
            _tarasClient
                .LoginAsync("testuser", "testpass", Arg.Any<CancellationToken>())
                .Returns(new LoginResponse { Success = true, Subscriber = new SubscriberInfo { SubscriberID = "123" } });

            var request = new LoginRequestDto { Username = "testuser", Password = "testpass" };

            var result = await _sut.LoginAsync(request);

            Assert.True(result.Success);
            Assert.Null(result.Error);
            Assert.Equal("123", result.Subscriber?.SubscriberID);
            Assert.Equal("testuser", _session.Username);
            await _tarasClient.Received(1).LoginAsync("testuser", "testpass", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsFail()
        {
            _tarasClient
                .LoginAsync("testuser", "wrongpass", Arg.Any<CancellationToken>())
                .Returns(new LoginResponse { Success = false, Error = "Invalid credentials.", IsCredentialError = true });

            var request = new LoginRequestDto { Username = "testuser", Password = "wrongpass" };
            var result = await _sut.LoginAsync(request);
            Assert.False(result.Success);
            Assert.Equal("Invalid credentials.", result.Error);
            Assert.True(result.IsCredentialError);
            Assert.Null(result.Subscriber);
            Assert.Null(_session.Username);
            await _tarasClient.Received(1).LoginAsync("testuser", "wrongpass", Arg.Any<CancellationToken>());
        }
    }
}
