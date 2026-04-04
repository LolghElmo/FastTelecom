using FastTelecom.Application.DTOs;
using FastTelecom.Domain.Interfaces;
using FastTelecom.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Application.Services
{
    public sealed class AuthenticationService
    {
        private readonly ITarasClient _tarasClient;
        private readonly SessionStore _session;

        public AuthenticationService(ITarasClient tarasClient, SessionStore session)
        {
            _tarasClient = tarasClient;
            _session     = session;
        }

        public async Task<LoginResultDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return LoginResultDto.Fail("Username is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return LoginResultDto.Fail("Password is required.");
            var response = await _tarasClient.LoginAsync(
                request.Username,
                request.Password,
                cancellationToken);

            if (!response.Success)
                return LoginResultDto.Fail(response.Error ?? "Login failed.", response.IsCredentialError);
            _session.Username = request.Username;
            _session.Password = request.Password;
            return new LoginResultDto
            {
                Success    = true,
                Subscriber = MapToDto(response.Subscriber),
            };
        }

        private static SubscriberDto? MapToDto(SubscriberInfo? info)
        {
            if (info is null) return null;

            return new SubscriberDto
            {
                SubscriberID = info.SubscriberID,
                ChargingType = info.ChargingType,
                EffectTime = info.EffectTime,
                ExpireTime = info.ExpireTime,
                FirstUseTime = info.FirstUseTime,
                LastUseEndTime = info.LastUseEndTime,
                UserProfileID = info.UserProfileID,
                IPAddress = info.IPAddress,
                ContactTeleNo = info.ContactTeleNo,
                Status = info.Status,
                MaxSessNumber = info.MaxSessNumber,
            };
        }
    }

}
