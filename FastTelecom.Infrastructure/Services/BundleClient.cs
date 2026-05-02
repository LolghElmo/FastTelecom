using FastTelecom.Domain.Interfaces;
using FastTelecom.Domain.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastTelecom.Infrastructure.Services
{
    public sealed class BundleClient : IBundleClient
    {
        private const string BundleListUrl =
            "https://www.syriantelecom.com.sy/php/Con_Flex1a225_5_CCBS2.php";

        private const string PurchaseUrl =
            "https://www.syriantelecom.com.sy//Sync/abtw225_5_send.php";

        private readonly HttpClient _http;
        private readonly ICryptoService _crypto;

        public BundleClient(HttpClient http, ICryptoService crypto)
        {
            _http = http;
            _crypto = crypto;
        }

        public async Task<BundlesApiResponse?> GetBundlesAsync(
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
                ["F_ID"] = "6",
                ["userName"] = userName,
                ["userPswd"] = userPswd,
                ["uNC"] = uNC,
                ["uPC"] = uPC,
            };

            var url = $"{BundleListUrl}?Rand={rand}";

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

                return JsonSerializer.Deserialize<BundlesApiResponse>(StripBom(body), JsonOpts);
            }
            catch (OperationCanceledException) { throw; }
            catch { return null; }
        }

        public async Task<PurchaseApiResponse> PurchaseBundleAsync(
            string username,
            string password,
            long basic,
            long bundleId,
            CancellationToken cancellationToken = default)
        {
            var rand = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var uNC = _crypto.Encrypt(username);
            var uPC = _crypto.Encrypt("TWAB4315!!!");

            var asRequest = JsonSerializer.Serialize(new
            {
                user = username,
                uNC,
                uPC,
                Basic = basic.ToString(),
                Products = new[] { bundleId },
            });

            var formData = new Dictionary<string, string>
            {
                ["x"] = "1",
                ["as_request"] = asRequest,
                ["Products_parent"] = "5",
                ["LangCo"] = "1",
            };

            var url = $"{PurchaseUrl}?Rand={rand}";

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
                    return new PurchaseApiResponse
                    {
                        Success = false,
                        Error = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                    };

                var trimmed = StripBom(body).TrimStart('[').TrimEnd(']').Trim();
                var wrapper = JsonSerializer.Deserialize<PurchaseResponseWrapper>(trimmed, JsonOpts);
                var raw = wrapper?.Response?.FirstOrDefault();

                return new PurchaseApiResponse
                {
                    Success = true,
                    Item = raw is null ? null : new PurchaseItemResult
                    {
                        ProductId = raw.ProductId,
                        Phone = raw.Phone,
                        Vol = raw.Vol,
                        Code = raw.Code,
                        Result = raw.Result,
                        Msg = raw.Msg,
                    },
                };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return new PurchaseApiResponse { Success = false, Error = ex.Message };
            }
        }

        private static string StripBom(string s) => s.Trim().TrimStart('﻿').Trim();

        // Helpers

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

        private sealed class PurchaseResponseWrapper
        {
            [JsonPropertyName("response")]
            public PurchaseItemRaw[]? Response { get; set; }
        }

        private sealed class PurchaseItemRaw
        {
            [JsonPropertyName("productID")]
            public string? ProductId { get; set; }
            [JsonPropertyName("phone")]
            public string? Phone { get; set; }
            [JsonPropertyName("vol")]
            public int Vol { get; set; }
            [JsonPropertyName("code")]
            public int Code { get; set; }
            [JsonPropertyName("result")]
            public string? Result { get; set; }
            [JsonPropertyName("msg")]
            public string? Msg { get; set; }
        }
    }
}
