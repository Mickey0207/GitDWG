using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using GitDWG.Services;
using GitDWG.Views;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GitDWG
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public MainWindow? MainWindow { get; private set; }
        private UserSettingsService? _userSettingsService;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            _userSettingsService = new UserSettingsService();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // 每次啟動都需要用戶登入
            await ShowUserLogin();
        }

        private async Task ShowUserLogin()
        {
            var loginWindow = new UserLoginWindow();
            var loginSuccessful = await loginWindow.ShowAsync();

            if (loginSuccessful && loginWindow.UserSettings != null)
            {
                // 登入成功，顯示主窗口
                await ShowMainWindow();
            }
            else
            {
                // 登入失敗或被取消，退出應用程式
                Application.Current.Exit();
            }
        }

        private async Task ShowInitialSetup()
        {
            var setupWindow = new InitialSetupWindow();
            var setupCompleted = await setupWindow.ShowAsync();

            if (setupCompleted && setupWindow.UserSettings != null)
            {
                // 設定完成，顯示主窗口
                await ShowMainWindow();
            }
            else
            {
                // 設定被取消，退出應用程式
                Application.Current.Exit();
            }
        }

        private async Task ShowMainWindow()
        {
            // 獲取用戶設定
            var userSettings = _userSettingsService!.LoadSettings();
            
            if (userSettings == null)
            {
                // 如果還是沒有設定，重新顯示登入畫面
                await ShowUserLogin();
                return;
            }

            // 創建主窗口並傳遞用戶設定
            MainWindow = new MainWindow(userSettings);
            MainWindow.Activate();
        }

        // 提供全域訪問用戶設定的方法
        public UserSettings? GetCurrentUserSettings()
        {
            return _userSettingsService?.LoadSettings();
        }

        public void UpdateUserSettings(UserSettings settings)
        {
            _userSettingsService?.SaveSettings(settings);
        }
    }
}
