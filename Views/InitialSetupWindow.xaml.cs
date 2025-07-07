using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GitDWG.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;

namespace GitDWG.Views
{
    public sealed class InitialSetupWindow : Window
    {
        public UserSettings? UserSettings { get; private set; }
        public bool SetupCompleted { get; private set; } = false;

        private readonly UserSettingsService _settingsService;
        private TaskCompletionSource<bool>? _setupCompletionSource;

        // 控制項
        private TextBox? _authorNameTextBox;
        private TextBox? _authorEmailTextBox;
        private TextBlock? _authorNameError;
        private TextBlock? _authorEmailError;
        private Button? _saveButton;

        public InitialSetupWindow()
        {
            _settingsService = new UserSettingsService();
            
            // 設定窗口屬性
            this.Title = "GitDWG - 初始設定";
            
            // 創建UI
            CreateUI();
            
            // 訂閱窗口關閉事件
            this.Closed += Window_Closed;

            // 設定背景
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

            // 載入現有設定
            LoadExistingSettings();
            
            this.Activate();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 17, 24, 39)) // 深色背景
            };
            
            var border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                Padding = new Thickness(32),
                MinWidth = 450,
                MinHeight = 380,
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
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 標題
            var titleBlock = new TextBlock
            {
                Text = "歡迎使用 GitDWG",
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)) // 淺色文字
            };
            Grid.SetRow(titleBlock, 0);
            contentGrid.Children.Add(titleBlock);

            var subtitleBlock = new TextBlock
            {
                Text = "請設定您的作者資訊，這將用於Git提交記錄。",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 32),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 148, 163, 184)) // 灰色文字
            };
            Grid.SetRow(subtitleBlock, 1);
            contentGrid.Children.Add(subtitleBlock);

            // 作者姓名
            var namePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var nameLabel = new TextBlock
            {
                Text = "作者姓名 *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // 淺灰色標籤
            };
            _authorNameTextBox = new TextBox
            {
                PlaceholderText = "請輸入您的姓名",
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // 深色輸入框背景
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // 白色文字
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            _authorNameTextBox.TextChanged += ValidateInput;

            _authorNameError = new TextBlock
            {
                Text = "請輸入作者姓名",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // 淺紅色錯誤提示
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Visibility = Visibility.Collapsed
            };

            namePanel.Children.Add(nameLabel);
            namePanel.Children.Add(_authorNameTextBox);
            namePanel.Children.Add(_authorNameError);
            Grid.SetRow(namePanel, 2);
            contentGrid.Children.Add(namePanel);

            // 作者信箱
            var emailPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var emailLabel = new TextBlock
            {
                Text = "作者信箱 *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // 淺灰色標籤
            };
            _authorEmailTextBox = new TextBox
            {
                PlaceholderText = "請輸入您的Email地址",
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // 深色輸入框背景
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // 白色文字
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            _authorEmailTextBox.TextChanged += ValidateInput;

            _authorEmailError = new TextBlock
            {
                Text = "請輸入有效的Email地址",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // 淺紅色錯誤提示
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Visibility = Visibility.Collapsed
            };

            emailPanel.Children.Add(emailLabel);
            emailPanel.Children.Add(_authorEmailTextBox);
            emailPanel.Children.Add(_authorEmailError);
            Grid.SetRow(emailPanel, 3);
            contentGrid.Children.Add(emailPanel);

            // 提示訊息 - 深色設計
            var infoPanel = new Border
            {
                Padding = new Thickness(16),
                Margin = new Thickness(0, 8, 0, 24),
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
                Text = "設定說明",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 209, 250, 229)) // 淺綠色標題
            };
            var infoText1 = new TextBlock
            {
                Text = "? 這些資訊將保存在本機，用於Git提交記錄",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // 淺綠色文字
            };
            var infoText2 = new TextBlock
            {
                Text = "? 您可以稍後在設定中修改這些資訊",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // 淺綠色文字
            };
            var infoText3 = new TextBlock
            {
                Text = "? 所有欄位都是必填的，完成後即可開始使用",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // 淺綠色文字
            };

            infoStack.Children.Add(infoTitle);
            infoStack.Children.Add(infoText1);
            infoStack.Children.Add(infoText2);
            infoStack.Children.Add(infoText3);
            infoPanel.Child = infoStack;
            Grid.SetRow(infoPanel, 4);
            contentGrid.Children.Add(infoPanel);

            // 按鈕區域 - 只保留確定按鈕
            var buttonPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 16, 0, 0)
            };

            _saveButton = new Button
            {
                Content = "完成設定",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 44,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)), // 綠色主要按鈕
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // 白色文字
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10),
                IsEnabled = false
            };
            _saveButton.Click += SaveButton_Click;

            // 添加Enter鍵支援
            _authorEmailTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Enter && _saveButton.IsEnabled)
                {
                    SaveButton_Click(s, null);
                }
            };

            buttonPanel.Children.Add(_saveButton);
            Grid.SetRow(buttonPanel, 5);
            contentGrid.Children.Add(buttonPanel);

            border.Child = contentGrid;
            mainGrid.Children.Add(border);
            this.Content = mainGrid;
        }

        private void LoadExistingSettings()
        {
            var existingSettings = _settingsService.LoadSettings();
            if (existingSettings != null && _authorNameTextBox != null && _authorEmailTextBox != null)
            {
                _authorNameTextBox.Text = existingSettings.AuthorName;
                _authorEmailTextBox.Text = existingSettings.AuthorEmail;
            }
            else if (_authorNameTextBox != null && _authorEmailTextBox != null)
            {
                // 如果沒有現有設定，使用預設值
                _authorNameTextBox.Text = Environment.UserName;
                _authorEmailTextBox.Text = $"{Environment.UserName}@example.com";
            }

            ValidateInput(null, null);
        }

        private void ValidateInput(object? sender, TextChangedEventArgs? e)
        {
            if (_authorNameTextBox == null || _authorEmailTextBox == null || 
                _authorNameError == null || _authorEmailError == null || _saveButton == null)
                return;

            bool isValid = true;

            // 驗證作者姓名
            if (string.IsNullOrWhiteSpace(_authorNameTextBox.Text))
            {
                _authorNameError.Visibility = Visibility.Visible;
                _authorNameError.Text = "請輸入作者姓名";
                isValid = false;
            }
            else if (_authorNameTextBox.Text.Trim().Length < 2)
            {
                _authorNameError.Visibility = Visibility.Visible;
                _authorNameError.Text = "作者姓名至少需要2個字元";
                isValid = false;
            }
            else
            {
                _authorNameError.Visibility = Visibility.Collapsed;
            }

            // 驗證Email
            if (string.IsNullOrWhiteSpace(_authorEmailTextBox.Text))
            {
                _authorEmailError.Visibility = Visibility.Visible;
                _authorEmailError.Text = "請輸入Email地址";
                isValid = false;
            }
            else if (!IsValidEmail(_authorEmailTextBox.Text))
            {
                _authorEmailError.Visibility = Visibility.Visible;
                _authorEmailError.Text = "請輸入有效的Email地址";
                isValid = false;
            }
            else
            {
                _authorEmailError.Visibility = Visibility.Collapsed;
            }

            _saveButton.IsEnabled = isValid;
            
            // 動態更新按鈕樣式
            if (isValid)
            {
                _saveButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)); // 啟用時的綠色
            }
            else
            {
                _saveButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)); // 禁用時的灰色
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_saveButton?.IsEnabled == true && _authorNameTextBox != null && _authorEmailTextBox != null)
            {
                UserSettings = new UserSettings
                {
                    AuthorName = _authorNameTextBox.Text.Trim(),
                    AuthorEmail = _authorEmailTextBox.Text.Trim(),
                    LastUpdated = DateTime.Now
                };

                _settingsService.SaveSettings(UserSettings);
                SetupCompleted = true;
                _setupCompletionSource?.SetResult(true);
                this.Close();
            }
        }

        public Task<bool> ShowAsync()
        {
            _setupCompletionSource = new TaskCompletionSource<bool>();
            this.Activate();
            return _setupCompletionSource.Task;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (!SetupCompleted)
            {
                // 如果設定未完成且窗口被關閉，退出應用程式
                Application.Current.Exit();
            }
        }
    }
}