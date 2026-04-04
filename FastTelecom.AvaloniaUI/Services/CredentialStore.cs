using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FastTelecom.AvaloniaUI.Services
{
    public sealed class CredentialStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FastTelecom", "session.dat");


        public (string Username, string Password)? TryLoad()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;

                var combined = File.ReadAllBytes(FilePath);
                if (combined.Length <= 16) return null;

                var iv         = combined[..16];
                var ciphertext = combined[16..];

                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                var plaintext = aes.DecryptCbc(ciphertext, iv);

                var doc = JsonDocument.Parse(plaintext);
                var u   = doc.RootElement.GetProperty("u").GetString();
                var p   = doc.RootElement.GetProperty("p").GetString();

                return u is null || p is null ? null : (u, p);
            }
            catch { return null; }
        }

        public void Save(string username, string password)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.GenerateIV();

                var json      = JsonSerializer.SerializeToUtf8Bytes(new { u = username, p = password });
                var encrypted = aes.EncryptCbc(json, aes.IV);

                var combined = new byte[aes.IV.Length + encrypted.Length];
                aes.IV.CopyTo(combined, 0);
                encrypted.CopyTo(combined, aes.IV.Length);

                File.WriteAllBytes(FilePath, combined);
            }
            catch { /* user wont be remembered */ }
        }
        public void Clear()
        {
            try { if (File.Exists(FilePath)) File.Delete(FilePath); }
            catch { }
        }
        private static byte[] DeriveKey()
        {
            var seed = $"{Environment.MachineName}:{Environment.UserName}:FastTelecom-v1";
            return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        }
    }
}
