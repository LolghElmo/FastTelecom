using FastTelecom.AvaloniaUI.ViewModels;
using System;
using System.IO;
using System.Text.Json;

namespace FastTelecom.AvaloniaUI.Services
{
    public sealed class UserPreferencesService
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FastTelecom", "preferences.json");

        private PreferencesData _data;

        public UserPreferencesService()
        {
            _data = Load();
        }

        public BundleSortMode BundleSortMode
        {
            get => _data.BundleSortMode;
            set
            {
                if (_data.BundleSortMode == value) return;
                _data.BundleSortMode = value;
                Save();
            }
        }

        private PreferencesData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<PreferencesData>(json) ?? new PreferencesData();
                }
            }
            catch { }
            return new PreferencesData();
        }

        private void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private sealed class PreferencesData
        {
            public BundleSortMode BundleSortMode { get; set; } = BundleSortMode.DateAscending;
        }
    }
}
