using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Domain.Models
{
    public sealed class LoginResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public SubscriberInfo? Subscriber { get; set; }
        public bool IsCredentialError { get; set; }
        public string? RawResponse { get; set; }
    }
}
