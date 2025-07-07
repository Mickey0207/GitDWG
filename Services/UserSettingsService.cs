using System;
using System.IO;
using System.Text.Json;

namespace GitDWG.Services
{
    public class UserSettingsService
    {
        private readonly string _settingsPath;
        private UserSettings? _currentSettings;

        public UserSettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDirectory = Path.Combine(appDataPath, "GitDWG");
            
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }
            
            _settingsPath = Path.Combine(appDirectory, "user_settings.json");
        }

        public UserSettings? LoadSettings()
        {
            if (_currentSettings != null)
                return _currentSettings;

            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _currentSettings = JsonSerializer.Deserialize<UserSettings>(json);
                    return _currentSettings;
                }
            }
            catch (Exception)
            {
                // 如果讀取失敗，返回null
            }

            return null;
        }

        public void SaveSettings(UserSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_settingsPath, json);
                _currentSettings = settings;
            }
            catch (Exception)
            {
                // 靜默處理保存錯誤
            }
        }

        public UserSettings GetCurrentSettings()
        {
            return _currentSettings ?? new UserSettings();
        }

        public bool HasValidSettings()
        {
            var settings = LoadSettings();
            return settings != null && 
                   !string.IsNullOrWhiteSpace(settings.AuthorName) && 
                   !string.IsNullOrWhiteSpace(settings.AuthorEmail);
        }
    }

    public class UserSettings
    {
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string AutoCADPath { get; set; } = string.Empty;
        public string LastRepositoryPath { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}