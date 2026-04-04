namespace FastTelecom.Application.DTOs
{
    public sealed class BundlesResultDto
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public long Basic { get; init; }
        public BundleDto[] Bundles { get; init; } = [];

        public static BundlesResultDto Fail(string error) => new()
        {
            Success = false,
            Error   = error,
        };
    }
}
