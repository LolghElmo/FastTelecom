namespace FastTelecom.Application.DTOs
{
    public sealed class BundleDto
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public int VolGb { get; init; }
        public bool IsAvailable { get; init; }
    }
}
