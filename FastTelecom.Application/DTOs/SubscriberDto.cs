using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Application.DTOs
{
    public sealed class SubscriberDto
    {
        public string? SubscriberID { get; init; }
        public string? ChargingType { get; init; }
        public string? EffectTime { get; init; }
        public string? ExpireTime { get; init; }
        public string? FirstUseTime { get; init; }
        public string? LastUseEndTime { get; init; }
        public string? UserProfileID { get; init; }
        public string? IPAddress { get; init; }
        public string? ContactTeleNo { get; init; }
        public int Status { get; init; }
        public int MaxSessNumber { get; init; }
    }
}
