using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Application.DTOs
{
    public sealed class LoginResultDto
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public SubscriberDto? Subscriber { get; init; }
        public bool IsCredentialError { get; init; }
        public static LoginResultDto Fail(string error, bool isCredentialError = false) => new()
        {
            Success           = false,
            Error             = error,
            IsCredentialError = isCredentialError,
        };
    }
}
