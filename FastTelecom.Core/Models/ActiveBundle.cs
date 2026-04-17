namespace FastTelecom.Domain.Models
{
    public sealed class ActiveBundle
    {
        public string? ProductID { get; set; }
        public string? ProductName { get; set; }
        public int RatingMode { get; set; }
        public long MaxServiceUsage { get; set; }
        public long FreeVolume { get; set; }
        public string? EffTime { get; set; }
        public string? ExpTime { get; set; }
        public string? Speed { get; set; }
        public int OnlineSessionNum { get; set; }
        public ActiveBundleAccumulateInfo? AccumulateInfo { get; set; }
    }
}
