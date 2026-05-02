using FastTelecom.Application.DTOs;
using FastTelecom.Domain.Interfaces;
using System.Globalization;

namespace FastTelecom.Application.Services
{
    public sealed class BundleService
    {
        private readonly IBundleClient _client;
        private readonly ITarasClient _tarasClient;
        private readonly SessionStore _session;

        public BundleService(IBundleClient client, ITarasClient tarasClient, SessionStore session)
        {
            _client = client;
            _tarasClient = tarasClient;
            _session = session;
        }
        public async Task<BundlesResultDto> GetBundlesAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_session.Username) ||
                string.IsNullOrWhiteSpace(_session.Password))
                return BundlesResultDto.Fail("Session expired. Please log in again.");

            try
            {
                var response = await _client.GetBundlesAsync(
                    _session.Username, _session.Password, ct);

                if (response is null)
                    return BundlesResultDto.Fail("Failed to retrieve bundles from server.");

                return new BundlesResultDto
                {
                    Success = true,
                    Basic = response.Basic,
                    Bundles = response.Bundles?
                        .Select(b => new BundleDto
                        {
                            Id = b.Id,
                            Name = b.Name?.Trim() ?? string.Empty,
                            Price = b.Price,
                            VolGb = b.Vol,
                            IsAvailable = b.IsEnable == 1,
                        })
                        .ToArray() ?? [],
                };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { return BundlesResultDto.Fail(ex.Message); }
        }

        public async Task<PurchaseResultDto> PurchaseBundleAsync(
            long bundleId,
            long basic,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_session.Username) ||
                string.IsNullOrWhiteSpace(_session.Password))
                return PurchaseResultDto.Fail("Session expired. Please log in again.");

            try
            {
                var response = await _client.PurchaseBundleAsync(
                    _session.Username, _session.Password, basic, bundleId, ct);

                if (!response.Success)
                    return PurchaseResultDto.Fail(response.Error ?? "Purchase failed.");

                var item = response.Item;
                if (item is null)
                    return PurchaseResultDto.Fail("No response received from server.");

                bool success = item.Code == 200 && string.IsNullOrWhiteSpace(item.Msg);

                return new PurchaseResultDto
                {
                    Success = success,
                    Message = success ? "Bundle purchased successfully!" : null,
                    Error = success ? null : (string.IsNullOrWhiteSpace(item.Msg)
                                  ? "Purchase was unsuccessful. Please try again."
                                  : item.Msg),
                };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { return PurchaseResultDto.Fail(ex.Message); }
        }
        public async Task<ActiveBundlesResultDto> GetActiveBundlesAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_session.Username) ||
                string.IsNullOrWhiteSpace(_session.Password))
                return ActiveBundlesResultDto.Fail("Session expired. Please log in again.");

            try
            {
                var bundles = await _tarasClient.GetActiveBundlesAsync(
                    _session.Username, _session.Password, ct);

                if (bundles is null)
                    return ActiveBundlesResultDto.Fail("Failed to retrieve active bundles.");

                return new ActiveBundlesResultDto
                {
                    Success = true,
                    Bundles = bundles
                        .OrderBy(b => b.EffTime ?? string.Empty)
                        .Select(MapBundle)
                        .ToArray(),
                };
            }
            catch (OperationCanceledException) { throw; }

            catch (Exception ex) { return ActiveBundlesResultDto.Fail(ex.Message); }
        }
        private static ActiveBundleDto MapBundle(Domain.Models.ActiveBundle b)
        {
            bool isUnlimited = b.MaxServiceUsage <= 0;

            long totalKb = isUnlimited ? 0L : b.MaxServiceUsage * 1024L;
            long usedKb = isUnlimited ? 0L : Math.Max(totalKb - b.FreeVolume, 0L);
            double usedMb = usedKb / 1024.0;
            double totalMb = b.MaxServiceUsage;
            double pct = isUnlimited || totalKb == 0 ? 0
                       : Math.Min(usedKb / (double)totalKb * 100.0, 100.0);
            double monthlyUsedMb = (b.AccumulateInfo?.MonthAccuVolume ?? 0) / 1024.0;

            var expiryDate = ParseDate(b.ExpTime);
            bool expiringSoon = expiryDate.HasValue &&
                                (expiryDate.Value - DateTime.Now).TotalDays <= 7;

            var name = b.ProductName?.Trim() ?? string.Empty;

            return new ActiveBundleDto
            {
                ProductId = b.ProductID ?? string.Empty,
                Name = name,
                IsUnlimited = isUnlimited,
                IsVolumeBundle = !isUnlimited,
                PercentUsed = pct,
                PercentLabel = $"{pct:F0}%",
                UsedOfTotalDisplay = isUnlimited
                    ? string.Empty
                    : $"Used {FormatMb(usedMb)} of {FormatMb(totalMb)}",
                MonthlyUsedDisplay = $"This month: {FormatMb(monthlyUsedMb)} used",
                TotalDisplay = isUnlimited ? "Unlimited" : FormatMb(totalMb),
                EffectiveDate = FormatDateStr(b.EffTime),
                ExpiryDate = FormatDateStr(b.ExpTime),
                Speed = b.Speed ?? string.Empty,
                IsOnline = b.OnlineSessionNum > 0,
                IsExpiringSoon = expiringSoon,
                ExpiryDateValue = expiryDate,
                EffectiveDateValue = ParseDate(b.EffTime),
                VolumeMb = b.MaxServiceUsage,
                BundleType = InferBundleType(name),
            };
        }

        private static string InferBundleType(string name)
        {
            if (name.Contains("الافتراضية")) return "Basic";
            if (name.Contains("سرعة") || name.Contains("بوست") ||
                name.Contains("Speed") || name.Contains("Boost")) return "Speed Boost";
            if (name.Contains("ليل") || name.Contains("نايت") ||
                name.Contains("Night") || name.Contains("Nightly")) return "Nightly";
            if (name.Contains("لا محدود") || name.Contains("مفتوح") ||
                name.Contains("Unlimited")) return "Unlimited";
            return "Standard";
        }

        private static string FormatMb(double mb) =>
            mb >= 1024 ? $"{mb / 1024.0:F1} GB" : $"{mb:F0} MB";

        private static DateTime? ParseDate(string? raw)
        {
            if (raw is null || raw.Length < 14) return null;
            return DateTime.TryParseExact(raw, "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                ? dt : null;
        }

        private static string FormatDateStr(string? raw)
        {
            var dt = ParseDate(raw);
            return dt.HasValue ? dt.Value.ToString("MMM d, yyyy") : "-";
        }
    }
}
