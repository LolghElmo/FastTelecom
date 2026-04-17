using FastTelecom.Domain.Models;

namespace FastTelecom.Domain.Interfaces
{
    public interface ITarasClient
    {
        Task<LoginResponse> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);

        Task<ActiveBundle[]?> GetActiveBundlesAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);
    }
}
