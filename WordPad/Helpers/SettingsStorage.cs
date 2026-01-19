using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace WordPad.Helpers
{
    /// <summary>
    /// Settings storage helper that works for both packaged and unpackaged apps
    /// </summary>
    public static class SettingsStorage
    {
        private static readonly string SettingsFilePath;
        private static readonly Dictionary<string, object> _settingsCache = new();
        private static bool _isPackaged;

        static SettingsStorage()
        {
            // Check if app is packaged
            try
            {
                var _ = ApplicationData.Current;
                _isPackaged = true;
            }
            catch
            {
                _isPackaged = false;
                // Use local app data folder for unpackaged apps
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(localAppData, "UltraPad");
                Directory.CreateDirectory(appFolder);
                SettingsFilePath = Path.Combine(appFolder, "settings.json");
                LoadSettings();
            }
        }

        public static object GetValue(string key)
        {
            if (_isPackaged)
            {
                return ApplicationData.Current.LocalSettings.Values[key];
            }
            else
            {
                return _settingsCache.TryGetValue(key, out var value) ? value : null;
            }
        }

        public static void SetValue(string key, object value)
        {
            if (_isPackaged)
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            else
            {
                _settingsCache[key] = value;
                SaveSettings();
            }
        }

        public static StorageFolder GetTemporaryFolder()
        {
            if (_isPackaged)
            {
                return ApplicationData.Current.TemporaryFolder;
            }
            else
            {
                throw new NotSupportedException("TemporaryFolder is not supported in unpackaged mode. Use System.IO.Path.GetTempPath() instead.");
            }
        }

        private static void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    if (settings != null)
                    {
                        foreach (var kvp in settings)
                        {
                            // Convert JsonElement to appropriate type
                            _settingsCache[kvp.Key] = kvp.Value.ValueKind switch
                            {
                                JsonValueKind.String => kvp.Value.GetString(),
                                JsonValueKind.Number => kvp.Value.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                _ => kvp.Value.ToString()
                            };
                        }
                    }
                }
                catch
                {
                    // If settings file is corrupt, start fresh
                    _settingsCache.Clear();
                }
            }
        }

        private static void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settingsCache, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
