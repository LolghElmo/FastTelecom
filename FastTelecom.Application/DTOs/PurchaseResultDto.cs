namespace FastTelecom.Application.DTOs
{
    public sealed class PurchaseResultDto
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public string? Message { get; init; }
        public static PurchaseResultDto Fail(string error) => new()
        {
            Success = false,
            Error   = error,
        };
    }
}
