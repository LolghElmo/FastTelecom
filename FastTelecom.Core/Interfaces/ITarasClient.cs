using FastTelecom.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
