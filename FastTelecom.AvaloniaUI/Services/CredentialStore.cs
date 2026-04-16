using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FastTelecom.AvaloniaUI.Services
{
    public sealed class CredentialStore
    {
        private static readonly string SessionPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FastTelecom", "session.dat");

        private static readonly string AccountsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FastTelecom", "accounts.dat");

        public record SavedAccount(string Username, string Password);

        public (string Username, string Password)? TryLoad()
        {
            try
            {
                if (!File.Exists(SessionPath)) return null;

                var combined = File.ReadAllBytes(SessionPath);
                if (combined.Length <= 16) return null;

                var iv = combined[..16];
                var ciphertext = combined[16..];

                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                var plaintext = aes.DecryptCbc(ciphertext, iv);

                var doc = JsonDocument.Parse(plaintext);
                var u = doc.RootElement.GetProperty("u").GetString();
                var p = doc.RootElement.GetProperty("p").GetString();

                return u is null || p is null ? null : (u, p);
            }
            catch { return null; }
        }

        public void Save(string username, string password)
        {
            SaveSession(username, password);
            Upsert(username, password);
        }

        public void Clear()
        {
            try { if (File.Exists(SessionPath)) File.Delete(SessionPath); }
            catch { }
        }

        public List<SavedAccount> LoadAll()
        {
            try
            {
                if (!File.Exists(AccountsPath))
                {
                    var legacy = TryLoad();
                    if (legacy.HasValue)
                    {
                        var migrated = new List<SavedAccount> { new(legacy.Value.Username, legacy.Value.Password) };
                        SaveAll(migrated);
                        return migrated;
                    }
                    return [];
                }

                var combined = File.ReadAllBytes(AccountsPath);
                if (combined.Length <= 16) return [];

                var iv = combined[..16];
                var ciphertext = combined[16..];

                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                var plaintext = aes.DecryptCbc(ciphertext, iv);

                var list = JsonSerializer.Deserialize<List<AccountEntry>>(plaintext);
                return list?.Select(e => new SavedAccount(e.U, e.P)).ToList() ?? [];
            }
            catch { return []; }
        }

        public void Remove(string username)
        {
            var remaining = LoadAll().Where(a => a.Username != username).ToList();
            SaveAll(remaining);
        }

        private void SaveSession(string username, string password)
        {
            try
            {
                EnsureDir(SessionPath);

                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.GenerateIV();

                var json = JsonSerializer.SerializeToUtf8Bytes(new { u = username, p = password });
                var encrypted = aes.EncryptCbc(json, aes.IV);

                var combined = new byte[aes.IV.Length + encrypted.Length];
                aes.IV.CopyTo(combined, 0);
                encrypted.CopyTo(combined, aes.IV.Length);

                File.WriteAllBytes(SessionPath, combined);
            }
            catch { }
        }

        private void Upsert(string username, string password)
        {
            var list = LoadAll().Where(a => a.Username != username).ToList();
            list.Insert(0, new SavedAccount(username, password));
            SaveAll(list);
        }

        private void SaveAll(List<SavedAccount> accounts)
        {
            try
            {
                EnsureDir(AccountsPath);

                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.GenerateIV();

                var entries = accounts.Select(a => new AccountEntry { U = a.Username, P = a.Password }).ToList();
                var json = JsonSerializer.SerializeToUtf8Bytes(entries);
                var encrypted = aes.EncryptCbc(json, aes.IV);

                var combined = new byte[aes.IV.Length + encrypted.Length];
                aes.IV.CopyTo(combined, 0);
                encrypted.CopyTo(combined, aes.IV.Length);

                File.WriteAllBytes(AccountsPath, combined);
            }
            catch { }
        }

        private static void EnsureDir(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        private static byte[] DeriveKey()
        {
            var seed = $"{Environment.MachineName}:{Environment.UserName}:FastTelecom-v1";
            return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        }

        private sealed class AccountEntry
        {
            public string U { get; set; } = string.Empty;
            public string P { get; set; } = string.Empty;
        }
    }
}
