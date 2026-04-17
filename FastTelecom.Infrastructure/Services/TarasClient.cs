using FastTelecom.Domain.Interfaces;
using FastTelecom.Domain.Models;
using System.Text.Json;

namespace FastTelecom.Infrastructure.Services
{
    public sealed class TarasClient : ITarasClient
    {
        private const string BaseUrl =
     "https://www.syriantelecom.com.sy/include/TarasSelfPortal.php";

        private readonly HttpClient _http;
        private readonly ICryptoService _crypto;

        public TarasClient(HttpClient http, ICryptoService crypto)
        {
            _http = http;
            _crypto = crypto;
        }

        public async Task<LoginResponse> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var uNC = _crypto.Encrypt(username);
            var uPC = _crypto.Encrypt(password);
            var userName = _crypto.UserNameHash;
            var userPswd = _crypto.UserPswdHash;

            var rand = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var rndm = GenerateRandomString(6);

            var formData = new Dictionary<string, string>
            {
                ["rndm"] = rndm,
                ["x"] = "1",
                ["LangCo"] = "1",
                ["vSource"] = "1",
                ["isWeb"] = "1",
                ["F_ID"] = "1",
                ["userName"] = userName,
                ["userPswd"] = userPswd,
                ["uNC"] = uNC,
                ["uPC"] = uPC,
            };

            var url = $"{BaseUrl}?Rand={rand}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new FormUrlEncodedContent(formData),
                };

                request.Headers.Add("Origin", "https://www.syriantelecom.com.sy");
                request.Headers.Add("Referer", "https://www.syriantelecom.com.sy/");

                using var response = await _http.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Error = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                        RawResponse = body,
                    };
                }

                var trimmed = body.Trim();

                if (trimmed.Equals("NOTOK", StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Error = "Invalid username or password.",
                        IsCredentialError = true,
                        RawResponse = body,
                    };
                }

                if (trimmed.StartsWith('['))
                {
                    var arr = JsonSerializer.Deserialize<SubscriberInfo[]>(
                        trimmed, JsonOpts);

                    return new LoginResponse
                    {
                        Success = true,
                        Subscriber = arr?.FirstOrDefault(),
                        RawResponse = body,
                    };
                }

                var subscriber = JsonSerializer.Deserialize<SubscriberInfo>(
                    trimmed, JsonOpts);

                return new LoginResponse
                {
                    Success = true,
                    Subscriber = subscriber,
                    RawResponse = body,
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Error = ex.Message,
                };
            }
        }

        public async Task<ActiveBundle[]?> GetActiveBundlesAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var uNC = _crypto.Encrypt(username);
            var uPC = _crypto.Encrypt(password);
            var userName = _crypto.UserNameHash;
            var userPswd = _crypto.UserPswdHash;
            var rand = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var rndm = GenerateRandomString(6);

            var formData = new Dictionary<string, string>
            {
                ["rndm"] = rndm,
                ["x"] = "1",
                ["LangCo"] = "1",
                ["vSource"] = "1",
                ["isWeb"] = "1",
                ["F_ID"] = "3",
                ["userName"] = userName,
                ["userPswd"] = userPswd,
                ["uNC"] = uNC,
                ["uPC"] = uPC,
            };

            var url = $"{BaseUrl}?Rand={rand}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new FormUrlEncodedContent(formData),
                };
                request.Headers.Add("Origin", "https://www.syriantelecom.com.sy");
                request.Headers.Add("Referer", "https://www.syriantelecom.com.sy/");

                using var response = await _http.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return null;

                return JsonSerializer.Deserialize<ActiveBundle[]>(body.Trim(), JsonOpts);
            }
            catch (OperationCanceledException) { throw; }
            catch { return null; }
        }

        // Private helpers

        private static readonly Random _rng = new();

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[_rng.Next(chars.Length)])
                .ToArray());
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
        };
    }
}
