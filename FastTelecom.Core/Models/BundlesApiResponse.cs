namespace FastTelecom.Domain.Models
{
    public sealed class BundlesApiResponse
    {
        public long Basic { get; set; }
        public Bundle[]? Bundles { get; set; }
    }
}
