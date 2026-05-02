using FastTelecom.Domain.Models;

namespace FastTelecom.Domain.Interfaces
{
    public interface IBundleClient
    {
        Task<BundlesApiResponse?> GetBundlesAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);

        Task<PurchaseApiResponse> PurchaseBundleAsync(
            string username,
            string password,
            long basic,
            long bundleId,
            CancellationToken cancellationToken = default);
    }
}
