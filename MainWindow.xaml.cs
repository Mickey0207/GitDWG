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
using Windows.Foundation;
using Windows.Foundation.Collections;
using GitDWG.ViewModels;
using GitDWG.Services;
using GitDWG.Views;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GitDWG
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(UserSettings userSettings)
        {
            this.InitializeComponent();
            
            // 使用用戶設定創建ViewModel
            ViewModel = new MainViewModel(userSettings);
            
            // 設定窗口標題
            this.Title = $"GitDWG - {userSettings.AuthorName}";
        }

        private async void OpenDrawingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filePath)
            {
                await ViewModel.OpenDrawingAsync(filePath);
            }
        }

        // Git選單事件處理
        private void OpenBranchGraphManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ViewModel.RepositoryPath) || !ViewModel.IsRepositoryLoaded)
                {
                    ShowMessage("請先選擇或初始化Git儲存庫");
                    return;
                }

                // 創建GitService實例
                var gitService = new GitService(ViewModel.RepositoryPath);
                
                // 開啟分支圖形管理器
                var branchGraphWindow = new BranchGraphWindow(gitService);
                branchGraphWindow.Activate();
            }
            catch (Exception ex)
            {
                ShowMessage($"開啟分支圖形管理器失敗: {ex.Message}");
            }
        }

        // 專案選單事件處理
        private async void OpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ViewModel.RepositoryPath) && Directory.Exists(ViewModel.RepositoryPath))
                {
                    await Launcher.LaunchUriAsync(new Uri(ViewModel.RepositoryPath));
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "提示",
                        Content = "尚未選擇有效的專案資料夾",
                        CloseButtonText = "確定",
                        XamlRoot = this.Content.XamlRoot,
                        RequestedTheme = ElementTheme.Dark
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "錯誤",
                    Content = $"無法開啟專案資料夾: {ex.Message}",
                    CloseButtonText = "確定",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Dark
                };
                await dialog.ShowAsync();
            }
        }

        // 視窗選單事件處理
        private async void NewWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userSettings = ((App)App.Current).GetCurrentUserSettings();
                if (userSettings != null)
                {
                    var newWindow = new MainWindow(userSettings);
                    newWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "錯誤",
                    Content = $"無法建立新視窗: {ex.Message}",
                    CloseButtonText = "確定",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = ElementTheme.Dark
                };
                await dialog.ShowAsync();
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 說明選單事件處理
        private async void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "GitDWG 使用說明",
                Content = "GitDWG 是一個專為AutoCAD圖面檔案設計的Git版本控制工具。\n\n" +
                         "主要功能：\n" +
                         "? Git版本控制\n" +
                         "? AutoCAD圖面比較\n" +
                         "? 智慧檔案管理\n" +
                         "? 多用戶協作\n" +
                         "? 圖形化分支管理\n\n" +
                         "詳細說明請參考產品文檔。",
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }

        private async void ShowQuickStart_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "快速入門",
                Content = "GitDWG 快速入門步驟：\n\n" +
                         "1. 選擇或初始化Git儲存庫\n" +
                         "2. 設定AutoCAD路徑\n" +
                         "3. 修改CAD圖面檔案\n" +
                         "4. 使用快速提交功能\n" +
                         "5. 查看提交歷史和比較版本\n" +
                         "6. 使用分支圖形管理器管理分支\n\n" +
                         "更多詳細說明請查看完整文檔。",
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }

        private async void ShowKeyboardShortcuts_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "鍵盤快捷鍵",
                Content = "常用快捷鍵：\n\n" +
                         "Ctrl+O - 開啟儲存庫\n" +
                         "Ctrl+N - 初始化儲存庫\n" +
                         "F5 - 重新整理\n" +
                         "Ctrl+Shift+F5 - 強制重新整理\n" +
                         "Ctrl+Enter - 快速提交\n" +
                         "Ctrl+A - 暫存所有\n" +
                         "Ctrl+Shift+B - 建立新分支\n" +
                         "Ctrl+Shift+G - 分支圖形管理器\n" +
                         "Ctrl+H - 查看提交歷史\n" +
                         "Ctrl+P - 選項設定\n" +
                         "F1 - 說明",
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "檢查更新",
                Content = "目前版本：GitDWG v1.2.0\n\n" +
                         "最新更新：\n" +
                         "? 深色UI主題設計\n" +
                         "? 圖形化分支管理器\n" +
                         "? 移除未完成功能\n\n" +
                         "您目前使用的是最新版本！",
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }

        private async void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "關於 GitDWG",
                Content = "GitDWG v1.2.0\n\n" +
                         "專為AutoCAD設計的Git版本控制工具\n\n" +
                         "核心功能：\n" +
                         "? 圖形化分支管理器\n" +
                         "? 深色UI主題設計\n" +
                         "? 專業版本控制\n" +
                         "? CAD檔案智慧管理\n\n" +
                         "開發團隊：GitDWG Development Team\n" +
                         "技術支援：support@gitdwg.com\n\n" +
                         "? 2024 GitDWG. All rights reserved.\n\n" +
                         "感謝您使用 GitDWG！",
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }

        // 輔助方法
        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "訊息",
                Content = message,
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }
    }
}
