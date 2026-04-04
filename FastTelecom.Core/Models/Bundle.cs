namespace FastTelecom.Domain.Models
{
    public sealed class Bundle
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Vol { get; set; }
        public int IsEnable { get; set; }
        public int Status { get; set; }
    }
}
