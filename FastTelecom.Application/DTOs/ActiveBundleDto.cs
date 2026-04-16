using System;

namespace FastTelecom.Application.DTOs
{
    public sealed class ActiveBundleDto
    {
        public string ProductId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;

        public bool IsUnlimited { get; init; }
        public bool IsVolumeBundle { get; init; }
        public double PercentUsed { get; init; } 
        public string UsedOfTotalDisplay { get; init; } = string.Empty;
        public string PercentLabel { get; init; } = string.Empty; 
        public string MonthlyUsedDisplay { get; init; } = string.Empty; 
        public string TotalDisplay { get; init; } = string.Empty;
        public string EffectiveDate { get; init; } = string.Empty;
        public string ExpiryDate { get; init; } = string.Empty;
        public string Speed { get; init; } = string.Empty;
        public bool IsOnline { get; init; }
        public bool IsExpiringSoon { get; init; }
        public DateTime? ExpiryDateValue    { get; init; }
        public DateTime? EffectiveDateValue { get; init; }
        public long VolumeMb { get; init; }
        public string BundleType { get; init; } = "Standard";
    }
}
