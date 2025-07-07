using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace GitDWG.Services
{
    public class UserSettingsService : IDisposable
    {
        private readonly string _settingsPath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private UserSettings? _currentSettings;
        private bool _disposed;

        public UserSettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDirectory = Path.Combine(appDataPath, "GitDWG");
            
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }
            
            _settingsPath = Path.Combine(appDirectory, "user_settings.json");
            Debug.WriteLine($"UserSettingsService initialized with path: {_settingsPath}");
        }

        public async Task<UserSettings?> LoadSettingsAsync()
        {
            if (_currentSettings != null)
                return _currentSettings;

            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = await File.ReadAllTextAsync(_settingsPath).ConfigureAwait(false);
                    
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true,
                            AllowTrailingCommas = true
                        };
                        
                        _currentSettings = JsonSerializer.Deserialize<UserSettings>(json, options);
                        
                        if (_currentSettings != null)
                        {
                            Debug.WriteLine($"Settings loaded successfully for user: {_currentSettings.AuthorName}");
                            return _currentSettings;
                        }
                    }
                }
                
                Debug.WriteLine("No valid settings file found, returning null");
                return null;
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                
                // 備份損壞的檔案
                await BackupCorruptedSettingsAsync().ConfigureAwait(false);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                return null;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public UserSettings? LoadSettings()
        {
            return LoadSettingsAsync().GetAwaiter().GetResult();
        }

        public async Task SaveSettingsAsync(UserSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // 更新最後修改時間
                settings.LastUpdated = DateTime.Now;
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(settings, options);
                
                // 使用安全的檔案寫入方法
                await SafeWriteFileAsync(_settingsPath, json).ConfigureAwait(false);
                
                _currentSettings = settings;
                Debug.WriteLine($"Settings saved successfully for user: {settings.AuthorName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
                
                // 提供診斷資訊
                var fileInfo = GetSettingsFileInfo();
                Debug.WriteLine($"Settings file info: {fileInfo}");
                
                throw new InvalidOperationException($"無法保存用戶設定: {ex.Message}\n\n診斷資訊:\n{fileInfo}", ex);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public void SaveSettings(UserSettings settings)
        {
            SaveSettingsAsync(settings).GetAwaiter().GetResult();
        }

        private async Task BackupCorruptedSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var backupPath = $"{_settingsPath}.corrupted_{timestamp}";
                    File.Copy(_settingsPath, backupPath);
                    Debug.WriteLine($"Corrupted settings backed up to: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to backup corrupted settings: {ex.Message}");
            }
        }

        public UserSettings GetCurrentSettings()
        {
            return _currentSettings ?? CreateDefaultSettings();
        }

        public async Task<UserSettings> GetCurrentSettingsAsync()
        {
            var settings = await LoadSettingsAsync().ConfigureAwait(false);
            return settings ?? CreateDefaultSettings();
        }

        private UserSettings CreateDefaultSettings()
        {
            return new UserSettings
            {
                AuthorName = Environment.UserName,
                AuthorEmail = $"{Environment.UserName}@example.com",
                LastUpdated = DateTime.Now
            };
        }

        public async Task<bool> HasValidSettingsAsync()
        {
            var settings = await LoadSettingsAsync().ConfigureAwait(false);
            return IsSettingsValid(settings);
        }

        public bool HasValidSettings()
        {
            var settings = LoadSettings();
            return IsSettingsValid(settings);
        }

        private bool IsSettingsValid(UserSettings? settings)
        {
            return settings != null && 
                   !string.IsNullOrWhiteSpace(settings.AuthorName) && 
                   !string.IsNullOrWhiteSpace(settings.AuthorEmail) &&
                   IsValidEmail(settings.AuthorEmail);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetSettingsAsync()
        {
            await _fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (File.Exists(_settingsPath))
                {
                    // 備份現有設定
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var backupPath = $"{_settingsPath}.reset_{timestamp}";
                    File.Copy(_settingsPath, backupPath);
                    
                    // 刪除現有設定
                    File.Delete(_settingsPath);
                }
                
                _currentSettings = null;
                Debug.WriteLine("Settings reset successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resetting settings: {ex.Message}");
                return false;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public bool ResetSettings()
        {
            return ResetSettingsAsync().GetAwaiter().GetResult();
        }

        public async Task<UserSettingsValidationResult> ValidateSettingsAsync(UserSettings settings)
        {
            var result = new UserSettingsValidationResult();
            
            if (settings == null)
            {
                result.Errors.Add("設定物件不能為空");
                return result;
            }

            if (string.IsNullOrWhiteSpace(settings.AuthorName))
            {
                result.Errors.Add("作者姓名不能為空");
            }
            else if (settings.AuthorName.Length > 100)
            {
                result.Errors.Add("作者姓名不能超過100個字符");
            }

            if (string.IsNullOrWhiteSpace(settings.AuthorEmail))
            {
                result.Errors.Add("作者信箱不能為空");
            }
            else if (!IsValidEmail(settings.AuthorEmail))
            {
                result.Errors.Add("作者信箱格式不正確");
            }

            if (!string.IsNullOrWhiteSpace(settings.AutoCADPath))
            {
                if (!File.Exists(settings.AutoCADPath))
                {
                    result.Warnings.Add("指定的AutoCAD路徑不存在");
                }
                else if (!settings.AutoCADPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add("AutoCAD路徑應該指向可執行檔(.exe)");
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.LastRepositoryPath))
            {
                if (!Directory.Exists(settings.LastRepositoryPath))
                {
                    result.Warnings.Add("上次使用的儲存庫路徑不存在");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        public UserSettingsValidationResult ValidateSettings(UserSettings settings)
        {
            return ValidateSettingsAsync(settings).GetAwaiter().GetResult();
        }

        public string GetSettingsFilePath()
        {
            return _settingsPath;
        }

        public async Task<bool> ExportSettingsAsync(string exportPath)
        {
            if (string.IsNullOrWhiteSpace(exportPath))
                return false;

            try
            {
                var settings = await LoadSettingsAsync().ConfigureAwait(false);
                if (settings == null) return false;

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                // 確保目錄存在
                var directory = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 直接覆蓋檔案（如果存在）
                await SafeWriteFileAsync(exportPath, json).ConfigureAwait(false);
                Debug.WriteLine($"Settings exported to: {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting settings: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ImportSettingsAsync(string importPath)
        {
            try
            {
                if (!File.Exists(importPath)) 
                {
                    Debug.WriteLine($"Import file does not exist: {importPath}");
                    return false;
                }

                var json = await File.ReadAllTextAsync(importPath).ConfigureAwait(false);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.WriteLine("Import file is empty");
                    return false;
                }

                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };
                
                var settings = JsonSerializer.Deserialize<UserSettings>(json, options);
                
                if (settings != null)
                {
                    var validation = await ValidateSettingsAsync(settings).ConfigureAwait(false);
                    if (validation.IsValid)
                    {
                        await SaveSettingsAsync(settings).ConfigureAwait(false);
                        Debug.WriteLine($"Settings imported from: {importPath}");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"Imported settings validation failed: {validation.ErrorMessage}");
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to deserialize imported settings");
                }
                
                return false;
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"JSON error importing settings: {jsonEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error importing settings: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _fileLock?.Dispose();
            _disposed = true;
            
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 安全地寫入檔案，處理檔案鎖定和存在問題
        /// </summary>
        private async Task SafeWriteFileAsync(string filePath, string content)
        {
            const int maxRetries = 3;
            const int retryDelayMs = 100;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    // 確保目錄存在
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // 嘗試直接寫入
                    await File.WriteAllTextAsync(filePath, content).ConfigureAwait(false);
                    return; // 成功，退出重試迴圈
                }
                catch (IOException ioEx) when (retry < maxRetries - 1)
                {
                    Debug.WriteLine($"File write attempt {retry + 1} failed: {ioEx.Message}");
                    
                    // 等待一段時間後重試
                    await Task.Delay(retryDelayMs * (retry + 1)).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException uaEx) when (retry < maxRetries - 1)
                {
                    Debug.WriteLine($"File access attempt {retry + 1} failed: {uaEx.Message}");
                    
                    // 嘗試移除唯讀屬性
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            var attributes = File.GetAttributes(filePath);
                            if (attributes.HasFlag(FileAttributes.ReadOnly))
                            {
                                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                            }
                        }
                    }
                    catch (Exception attrEx)
                    {
                        Debug.WriteLine($"Failed to modify file attributes: {attrEx.Message}");
                    }
                    
                    await Task.Delay(retryDelayMs * (retry + 1)).ConfigureAwait(false);
                }
            }

            // 如果所有重試都失敗，拋出異常
            throw new InvalidOperationException($"無法寫入檔案 {filePath}，已嘗試 {maxRetries} 次");
        }

        /// <summary>
        /// 獲取設定檔案的詳細資訊，用於診斷
        /// </summary>
        public SettingsFileInfo GetSettingsFileInfo()
        {
            try
            {
                var info = new SettingsFileInfo
                {
                    FilePath = _settingsPath,
                    Exists = File.Exists(_settingsPath)
                };

                if (info.Exists)
                {
                    var fileInfo = new FileInfo(_settingsPath);
                    info.Size = fileInfo.Length;
                    info.LastModified = fileInfo.LastWriteTime;
                    info.IsReadOnly = fileInfo.IsReadOnly;
                    info.Attributes = fileInfo.Attributes;
                }

                // 檢查目錄權限
                var directory = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    info.DirectoryExists = Directory.Exists(directory);
                    
                    if (info.DirectoryExists)
                    {
                        // 嘗試創建測試檔案來檢查寫入權限
                        var testFile = Path.Combine(directory, $"test_{Guid.NewGuid()}.tmp");
                        try
                        {
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            info.HasWritePermission = true;
                        }
                        catch
                        {
                            info.HasWritePermission = false;
                        }
                    }
                }

                return info;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting settings file info: {ex.Message}");
                return new SettingsFileInfo
                {
                    FilePath = _settingsPath,
                    Exists = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    public class UserSettings
    {
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string AutoCADPath { get; set; } = string.Empty;
        public string LastRepositoryPath { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        // 新增的設定選項
        public bool AutoSaveEnabled { get; set; } = true;
        public int MaxCommitHistoryCount { get; set; } = 100;
        public bool ShowPerformanceMetrics { get; set; } = false;
        public string PreferredTheme { get; set; } = "Default";
        public bool EnableAutoRefresh { get; set; } = true;
        public TimeSpan AutoRefreshInterval { get; set; } = TimeSpan.FromSeconds(30);
    }

    public class UserSettingsValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        
        public bool HasWarnings => Warnings.Count > 0;
        public string ErrorMessage => string.Join("; ", Errors);
        public string WarningMessage => string.Join("; ", Warnings);
    }

    public class SettingsFileInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsReadOnly { get; set; }
        public FileAttributes Attributes { get; set; }
        public bool DirectoryExists { get; set; }
        public bool HasWritePermission { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return $"錯誤: {ErrorMessage}";
            }

            if (!Exists)
            {
                return $"檔案不存在: {FilePath}";
            }

            return $"檔案: {FilePath}\n" +
                   $"大小: {Size} bytes\n" +
                   $"最後修改: {LastModified:yyyy-MM-dd HH:mm:ss}\n" +
                   $"唯讀: {IsReadOnly}\n" +
                   $"目錄存在: {DirectoryExists}\n" +
                   $"寫入權限: {HasWritePermission}";
        }
    }
}