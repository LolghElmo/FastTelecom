namespace FastTelecom.Domain.Models
{
    public sealed class PurchaseApiResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public PurchaseItemResult? Item { get; set; }
    }
}
