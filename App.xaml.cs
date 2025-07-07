using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GitDWG.Services;
using GitDWG.Views;
using System.Diagnostics;

namespace GitDWG
{
    public partial class App : Application
    {
        public MainWindow? MainWindow { get; private set; }
        private UserSettingsService? _userSettingsService;
        private bool _isShuttingDown;

        public App()
        {
            this.InitializeComponent();
            
            // 添加全域異常處理
            this.UnhandledException += App_UnhandledException;
            
            // 安全地初始化用戶設定服務
            InitializeUserSettingsService();
        }

        private void InitializeUserSettingsService()
        {
            try
            {
                _userSettingsService = new UserSettingsService();
                Debug.WriteLine("UserSettingsService initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserSettingsService initialization failed: {ex.Message}");
                // 繼續執行，但沒有用戶設定服務
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (_isShuttingDown) return;

            // 記錄詳細的異常資訊
            Debug.WriteLine($"=== 未處理的異常 ===");
            Debug.WriteLine($"Exception: {e.Exception.GetType().Name}");
            Debug.WriteLine($"Message: {e.Exception.Message}");
            Debug.WriteLine($"StackTrace: {e.Exception.StackTrace}");
            Debug.WriteLine($"InnerException: {e.Exception.InnerException?.Message}");
            Debug.WriteLine($"========================");
            
            // 嘗試顯示用戶友善的錯誤訊息
            try
            {
                ShowErrorMessage(e.Exception);
                
                // 對於某些非致命錯誤，標記為已處理
                if (IsNonFatalException(e.Exception))
                {
                    e.Handled = true;
                    Debug.WriteLine("Exception marked as handled (non-fatal)");
                }
            }
            catch (Exception handlerEx)
            {
                Debug.WriteLine($"Error in exception handler: {handlerEx.Message}");
                // 如果連錯誤處理都失敗了，讓系統處理
            }
        }

        private bool IsNonFatalException(Exception exception)
        {
            // 定義哪些異常類型是非致命的
            return exception is ArgumentException ||
                   exception is InvalidOperationException ||
                   exception is System.IO.IOException ||
                   exception is UnauthorizedAccessException;
        }

        private void ShowErrorMessage(Exception exception)
        {
            var errorMessage = GetUserFriendlyErrorMessage(exception);
            Debug.WriteLine($"User-friendly error message: {errorMessage}");
            
            // 這裡可以擴展為顯示實際的對話框
            // 但現在先記錄日誌
        }

        private string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => "GitDWG 沒有足夠的權限訪問某些檔案或資料夾。請以管理員身份執行，或檢查檔案權限設定。",
                System.IO.IOException => "檔案系統操作失敗。可能是檔案被其他程式使用中，或磁碟空間不足。",
                ArgumentException => "程式參數錯誤。請檢查輸入的資料格式是否正確。",
                InvalidOperationException => "當前操作無效。請檢查操作順序或系統狀態。",
                _ => $"GitDWG 遇到未預期的錯誤:\n\n{exception.Message}\n\n請重新啟動應用程式。如果問題持續，請聯絡技術支援。"
            };
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("Application launched, starting user login...");
                await ShowUserLoginAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Critical error during launch: {ex.Message}");
                await HandleLaunchFailureAsync(ex);
            }
        }

        private async Task HandleLaunchFailureAsync(Exception ex)
        {
            try
            {
                Debug.WriteLine("Attempting to handle launch failure...");
                
                // 嘗試使用預設設定直接啟動主視窗
                var defaultSettings = CreateDefaultUserSettings();
                await ShowMainWindowWithSettingsAsync(defaultSettings);
                
                Debug.WriteLine("Successfully recovered from launch failure");
            }
            catch (Exception recoveryEx)
            {
                Debug.WriteLine($"Recovery attempt failed: {recoveryEx.Message}");
                await SafeExitAsync();
            }
        }

        private UserSettings CreateDefaultUserSettings()
        {
            return new UserSettings
            {
                AuthorName = Environment.UserName,
                AuthorEmail = $"{Environment.UserName}@example.com",
                LastUpdated = DateTime.Now
            };
        }

        private async Task ShowUserLoginAsync()
        {
            try
            {
                var loginWindow = new UserLoginWindow();
                var loginSuccessful = await loginWindow.ShowAsync();

                if (loginSuccessful && loginWindow.UserSettings != null)
                {
                    Debug.WriteLine("User login successful, showing main window...");
                    await ShowMainWindowAsync();
                }
                else
                {
                    Debug.WriteLine("User login cancelled, exiting application...");
                    await SafeExitAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowUserLogin failed: {ex.Message}");
                
                try
                {
                    Debug.WriteLine("Attempting fallback to main window with default settings...");
                    var defaultSettings = CreateDefaultUserSettings();
                    await ShowMainWindowWithSettingsAsync(defaultSettings);
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"Fallback to main window also failed: {fallbackEx.Message}");
                    await SafeExitAsync();
                }
            }
        }

        private async Task ShowInitialSetupAsync()
        {
            try
            {
                var setupWindow = new InitialSetupWindow();
                var setupCompleted = await setupWindow.ShowAsync();

                if (setupCompleted && setupWindow.UserSettings != null)
                {
                    Debug.WriteLine("Initial setup completed, showing main window...");
                    await ShowMainWindowAsync();
                }
                else
                {
                    Debug.WriteLine("Initial setup cancelled, exiting application...");
                    await SafeExitAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowInitialSetup failed: {ex.Message}");
                await SafeExitAsync();
            }
        }

        private async Task ShowMainWindowAsync()
        {
            try
            {
                var userSettings = LoadUserSettings();
                
                if (userSettings == null)
                {
                    Debug.WriteLine("No user settings found, showing login again...");
                    await ShowUserLoginAsync();
                    return;
                }

                await ShowMainWindowWithSettingsAsync(userSettings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowMainWindow failed: {ex.Message}");
                
                var defaultSettings = CreateDefaultUserSettings();
                await ShowMainWindowWithSettingsAsync(defaultSettings);
            }
        }

        private UserSettings? LoadUserSettings()
        {
            try
            {
                return _userSettingsService?.LoadSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load user settings: {ex.Message}");
                return null;
            }
        }

        private async Task ShowMainWindowWithSettingsAsync(UserSettings userSettings)
        {
            try
            {
                Debug.WriteLine("Creating main window...");
                MainWindow = new MainWindow(userSettings);
                MainWindow.Activate();
                Debug.WriteLine("Main window activated successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowMainWindowWithSettings failed: {ex.Message}");
                await ShowCriticalErrorWindowAsync(ex);
            }
        }

        private async Task ShowCriticalErrorWindowAsync(Exception ex)
        {
            try
            {
                Debug.WriteLine("Attempting to show critical error window...");
                
                var errorWindow = new Window
                {
                    Title = "GitDWG - 嚴重錯誤"
                };
                
                var scrollViewer = new ScrollViewer
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Margin = new Thickness(20)
                };

                var errorContent = new TextBlock
                {
                    Text = $"GitDWG 主視窗無法啟動，發生嚴重錯誤：\n\n" +
                           $"錯誤類型：{ex.GetType().Name}\n" +
                           $"錯誤訊息：{ex.Message}\n\n" +
                           $"可能的解決方案：\n" +
                           $"• 重新啟動應用程式\n" +
                           $"• 以管理員身份執行\n" +
                           $"• 檢查磁碟空間是否足夠\n" +
                           $"• 確認 .NET 8 運行時已正確安裝\n\n" +
                           $"如果問題持續，請聯絡技術支援並提供此錯誤訊息。\n\n" +
                           $"詳細堆疊追蹤：\n{ex.StackTrace}",
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                    FontSize = 12
                };
                
                scrollViewer.Content = errorContent;
                errorWindow.Content = scrollViewer;
                errorWindow.Activate();
                
                Debug.WriteLine("Critical error window displayed");
            }
            catch (Exception criticalEx)
            {
                Debug.WriteLine($"Failed to show critical error window: {criticalEx.Message}");
                await SafeExitAsync();
            }
        }

        private async Task SafeExitAsync()
        {
            if (_isShuttingDown) return;
            
            _isShuttingDown = true;
            Debug.WriteLine("Initiating safe application shutdown...");
            
            try
            {
                // 清理資源
                await CleanupResourcesAsync();
                
                // 安全退出
                Exit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during safe exit: {ex.Message}");
                Environment.Exit(-1);
            }
        }

        private async Task CleanupResourcesAsync()
        {
            try
            {
                Debug.WriteLine("Cleaning up application resources...");
                
                // 清理主視窗
                if (MainWindow != null)
                {
                    try
                    {
                        MainWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error closing main window: {ex.Message}");
                    }
                }

                // 清理用戶設定服務
                _userSettingsService = null;
                
                // 強制垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
                
                Debug.WriteLine("Resource cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during resource cleanup: {ex.Message}");
            }
        }

        // 提供全域訪問用戶設定的方法
        public UserSettings? GetCurrentUserSettings()
        {
            try
            {
                return _userSettingsService?.LoadSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCurrentUserSettings failed: {ex.Message}");
                return null;
            }
        }

        public void UpdateUserSettings(UserSettings settings)
        {
            try
            {
                _userSettingsService?.SaveSettings(settings);
                Debug.WriteLine("User settings updated successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateUserSettings failed: {ex.Message}");
            }
        }

        // 提供應用程式級別的診斷資訊
        public AppDiagnosticInfo GetDiagnosticInfo()
        {
            try
            {
                return new AppDiagnosticInfo
                {
                    Version = "1.3.0",
                    FrameworkVersion = Environment.Version.ToString(),
                    OSVersion = Environment.OSVersion.ToString(),
                    WorkingDirectory = Environment.CurrentDirectory,
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    IsMainWindowActive = MainWindow != null,
                    HasUserSettingsService = _userSettingsService != null,
                    TotalMemory = GC.GetTotalMemory(false),
                    CollectionCount = GC.CollectionCount(0)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting diagnostic info: {ex.Message}");
                return new AppDiagnosticInfo { Error = ex.Message };
            }
        }
    }

    // 診斷資訊類別
    public class AppDiagnosticInfo
    {
        public string Version { get; set; } = string.Empty;
        public string FrameworkVersion { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsMainWindowActive { get; set; }
        public bool HasUserSettingsService { get; set; }
        public long TotalMemory { get; set; }
        public int CollectionCount { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
