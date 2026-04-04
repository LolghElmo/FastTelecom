namespace FastTelecom.Domain.Models
{
    public sealed class PurchaseItemResult
    {
        public string? ProductId { get; set; }
        public string? Phone { get; set; }
        public int Vol { get; set; }
        public int Code { get; set; }
        public string? Result { get; set; }
        public string? Msg { get; set; }
    }
}
