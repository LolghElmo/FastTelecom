using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Domain.Models
{
    public sealed class SubscriberInfo
    {
        public string? SubscriberID { get; set; }
        public int SubscriberIDType { get; set; }
        public int PermittedANTType { get; set; }
        public int AccessPolicyID { get; set; }
        public string? ChargingType { get; set; }
        public int MaxSessNumber { get; set; }
        public string? EffectTime { get; set; }
        public string? ExpireTime { get; set; }
        public int BindingUEMode { get; set; }
        public int RoamingLevel { get; set; }
        public int HWIDType { get; set; }
        public int MSType { get; set; }
        public int LimitedAccessLocation { get; set; }
        public int NASInfoBindingType { get; set; }
        public int Status { get; set; }
        public int HasFramedRouteList { get; set; }
        public string? FirstUseTime { get; set; }
        // nullable integer
        public int? UserProfileID { get; set; }
        public string? LastUseEndTime { get; set; }
        public string? ContactTeleNo { get; set; }
        public string? IPAddress { get; set; }
        public string? VtpServerDate { get; set; }
        public string? PassMd5 { get; set; }
    }
}
