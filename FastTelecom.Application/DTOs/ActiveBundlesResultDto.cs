namespace FastTelecom.Application.DTOs
{
    public sealed class ActiveBundlesResultDto
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public ActiveBundleDto[] Bundles { get; init; } = [];

        public static ActiveBundlesResultDto Fail(string error) => new()
        {
            Success = false,
            Error   = error,
        };
    }
}
