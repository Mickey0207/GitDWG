using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GitDWG.Services;
using GitDWG.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace GitDWG.Views
{
    public sealed class UserLoginWindow : Window
    {
        public UserSettings? UserSettings { get; private set; }
        public bool LoginSuccessful { get; private set; } = false;

        private readonly UserAuthenticationService _authService;
        private readonly UserSettingsService _settingsService;
        private TaskCompletionSource<bool>? _loginCompletionSource;

        // 控制項
        private ComboBox? _userComboBox;
        private PasswordBox? _passwordBox;
        private TextBlock? _errorMessage;
        private Button? _loginButton;
        private Button? _addUserButton;

        public UserLoginWindow()
        {
            _authService = new UserAuthenticationService();
            _settingsService = new UserSettingsService();
            
            // 設定窗口屬性
            this.Title = "GitDWG - 用戶登入";
            
            // 創建UI
            CreateUI();
            
            // 訂閱窗口關閉事件
            this.Closed += Window_Closed;

            // 設定背景
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            
            this.Activate();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 17, 24, 39)) // 深色主背景
            };
            
            var border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                Padding = new Thickness(32),
                MinWidth = 450,
                MinHeight = 400,
                CornerRadius = new CornerRadius(16),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 30, 41, 59)), // 深灰藍背景
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(80, 59, 130, 246)), // 藍色邊框
                BorderThickness = new Thickness(1)
            };

            var contentGrid = new Grid();
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 標題和圖標
            var titlePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 24)
            };
            
            var titleBlock = new TextBlock
            {
                Text = "GitDWG 用戶登入",
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)) // 淺色文字
            };
            
            titlePanel.Children.Add(titleBlock);
            Grid.SetRow(titlePanel, 0);
            contentGrid.Children.Add(titlePanel);

            var subtitleBlock = new TextBlock
            {
                Text = "請選擇您的用戶名稱並輸入密碼",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 32),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 148, 163, 184)) // 灰色文字
            };
            Grid.SetRow(subtitleBlock, 1);
            contentGrid.Children.Add(subtitleBlock);

            // 用戶選擇
            var userPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var userLabel = new TextBlock
            {
                Text = "選擇用戶",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // 淺灰色標籤
            };
            
            _userComboBox = new ComboBox
            {
                PlaceholderText = "請選擇用戶",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // 深色背景
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // 白色文字
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            
            // 載入用戶列表
            LoadUsers();

            userPanel.Children.Add(userLabel);
            userPanel.Children.Add(_userComboBox);
            Grid.SetRow(userPanel, 2);
            contentGrid.Children.Add(userPanel);

            // 密碼輸入
            var passwordPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var passwordLabel = new TextBlock
            {
                Text = "輸入密碼",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // 淺灰色標籤
            };
            
            _passwordBox = new PasswordBox
            {
                PlaceholderText = "請輸入密碼",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // 深色背景
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // 白色文字
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            _passwordBox.KeyDown += PasswordBox_KeyDown;

            passwordPanel.Children.Add(passwordLabel);
            passwordPanel.Children.Add(_passwordBox);
            Grid.SetRow(passwordPanel, 3);
            contentGrid.Children.Add(passwordPanel);

            // 錯誤訊息
            _errorMessage = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // 淺紅色錯誤
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 16),
                Visibility = Visibility.Collapsed,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(_errorMessage, 4);
            contentGrid.Children.Add(_errorMessage);

            // 按鈕區域
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Margin = new Thickness(0, 16, 0, 0)
            };

            _loginButton = new Button
            {
                Content = "登入",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 44,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)), // 綠色主要按鈕
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // 白色文字
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10)
            };
            _loginButton.Click += LoginButton_Click;

            _addUserButton = new Button
            {
                Content = "新增用戶",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 99, 102, 241)), // 藍色次要按鈕
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // 白色文字
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(8)
            };
            _addUserButton.Click += AddUserButton_Click;

            var exitButton = new Button
            {
                Content = "退出應用程式",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // 淺紅色危險按鈕
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // 白色文字
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(8)
            };
            exitButton.Click += ExitButton_Click;

            buttonPanel.Children.Add(_loginButton);
            buttonPanel.Children.Add(_addUserButton);
            buttonPanel.Children.Add(exitButton);
            Grid.SetRow(buttonPanel, 5);
            contentGrid.Children.Add(buttonPanel);

            // 說明文字 - 深色設計
            var infoPanel = new Border
            {
                Padding = new Thickness(16),
                Margin = new Thickness(0, 24, 0, 0),
                CornerRadius = new CornerRadius(8),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(40, 16, 185, 129)), // 半透明綠色背景
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(60, 16, 185, 129)), // 綠色邊框
                BorderThickness = new Thickness(1)
            };
            
            var infoStack = new StackPanel { Spacing = 6 };
            var infoTitle = new TextBlock
            {
                Text = "使用說明",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 209, 250, 229)) // 淺綠色標題
            };
            var infoText1 = new TextBlock
            {
                Text = "? 選擇您的用戶名稱並輸入對應密碼",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // 淺綠色文字
            };
            var infoText2 = new TextBlock
            {
                Text = "? 新增用戶需要管理員密碼授權",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // 淺綠色文字
            };
            var infoText3 = new TextBlock
            {
                Text = "? 登入成功後將進入Git版本控制系統",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // 淺綠色文字
            };

            infoStack.Children.Add(infoTitle);
            infoStack.Children.Add(infoText1);
            infoStack.Children.Add(infoText2);
            infoStack.Children.Add(infoText3);
            infoPanel.Child = infoStack;
            Grid.SetRow(infoPanel, 6);
            contentGrid.Children.Add(infoPanel);

            border.Child = contentGrid;
            mainGrid.Children.Add(border);
            this.Content = mainGrid;
        }

        private void LoadUsers()
        {
            if (_userComboBox == null) return;

            _userComboBox.Items.Clear();
            var users = _authService.GetAllUsers().OrderBy(u => u.Name);
            
            foreach (var user in users)
            {
                _userComboBox.Items.Add(user.Name);
            }
        }

        private void PasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginButton_Click(sender, null);
            }
        }

        private void LoginButton_Click(object? sender, RoutedEventArgs? e)
        {
            if (_userComboBox?.SelectedItem == null)
            {
                ShowError("請選擇用戶");
                return;
            }

            if (string.IsNullOrEmpty(_passwordBox?.Password))
            {
                ShowError("請輸入密碼");
                return;
            }

            var userName = _userComboBox.SelectedItem.ToString();
            var password = _passwordBox.Password;

            if (_authService.AuthenticateUser(userName!, password))
            {
                // 登入成功，創建UserSettings
                var user = _authService.GetUser(userName!);
                UserSettings = new UserSettings
                {
                    AuthorName = user!.Name,
                    AuthorEmail = $"{user.Name.ToLower()}@company.com",
                    LastUpdated = DateTime.Now
                };

                _settingsService.SaveSettings(UserSettings);
                LoginSuccessful = true;
                _loginCompletionSource?.SetResult(true);
                this.Close();
            }
            else
            {
                ShowError("用戶名稱或密碼錯誤，請重新輸入");
                _passwordBox.Password = string.Empty;
                _passwordBox.Focus(FocusState.Programmatic);
            }
        }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowAddUserDialog();
        }

        private async Task ShowAddUserDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "新增用戶",
                PrimaryButtonText = "新增",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark // 深色對話框
            };

            var panel = new StackPanel { Spacing = 16 };
            
            var adminPasswordBox = new PasswordBox
            {
                Header = "管理員密碼",
                PlaceholderText = "請輸入管理員密碼",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99))
            };
            
            var newUserNameBox = new TextBox
            {
                Header = "新用戶名稱",
                PlaceholderText = "請輸入新用戶名稱",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99))
            };
            
            var newUserPasswordBox = new PasswordBox
            {
                Header = "新用戶密碼",
                PlaceholderText = "請輸入新用戶密碼",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99))
            };

            panel.Children.Add(adminPasswordBox);
            panel.Children.Add(newUserNameBox);
            panel.Children.Add(newUserPasswordBox);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                if (!_authService.AuthenticateAdmin(adminPasswordBox.Password))
                {
                    ShowError("管理員密碼錯誤");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newUserNameBox.Text))
                {
                    ShowError("請輸入用戶名稱");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newUserPasswordBox.Password))
                {
                    ShowError("請輸入用戶密碼");
                    return;
                }

                if (_authService.AddUser(newUserNameBox.Text.Trim(), newUserPasswordBox.Password))
                {
                    LoadUsers(); // 重新載入用戶列表
                    ShowSuccess($"用戶 '{newUserNameBox.Text.Trim()}' 新增成功！");
                }
                else
                {
                    ShowError("用戶已存在或新增失敗");
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void ShowError(string message)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Text = "錯誤: " + message;
                _errorMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)); // 淺紅色
                _errorMessage.Visibility = Visibility.Visible;
            }
        }

        private void ShowSuccess(string message)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Text = "成功: " + message;
                _errorMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)); // 綠色
                _errorMessage.Visibility = Visibility.Visible;
            }
        }

        public Task<bool> ShowAsync()
        {
            _loginCompletionSource = new TaskCompletionSource<bool>();
            this.Activate();
            return _loginCompletionSource.Task;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (!LoginSuccessful)
            {
                // 如果登入未成功且窗口被關閉，退出應用程式
                Application.Current.Exit();
            }
        }
    }
}