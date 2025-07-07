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
            var mainGrid = new Grid();
            
            var border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                Padding = new Thickness(24),
                MinWidth = 400,
                MinHeight = 300,
                CornerRadius = new CornerRadius(8)
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
                FontSize = 24,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(titleBlock, 0);
            contentGrid.Children.Add(titleBlock);

            var subtitleBlock = new TextBlock
            {
                Text = "請設定您的作者資訊，這將用於Git提交記錄。",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 24),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(subtitleBlock, 1);
            contentGrid.Children.Add(subtitleBlock);

            // 作者姓名
            var namePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var nameLabel = new TextBlock
            {
                Text = "作者姓名 *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            _authorNameTextBox = new TextBox
            {
                PlaceholderText = "請輸入您的姓名"
            };
            _authorNameTextBox.TextChanged += ValidateInput;

            _authorNameError = new TextBlock
            {
                Text = "請輸入作者姓名",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0),
                Visibility = Visibility.Collapsed
            };

            namePanel.Children.Add(nameLabel);
            namePanel.Children.Add(_authorNameTextBox);
            namePanel.Children.Add(_authorNameError);
            Grid.SetRow(namePanel, 2);
            contentGrid.Children.Add(namePanel);

            // 作者信箱
            var emailPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var emailLabel = new TextBlock
            {
                Text = "作者信箱 *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            _authorEmailTextBox = new TextBox
            {
                PlaceholderText = "請輸入您的Email地址"
            };
            _authorEmailTextBox.TextChanged += ValidateInput;

            _authorEmailError = new TextBlock
            {
                Text = "請輸入有效的Email地址",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0),
                Visibility = Visibility.Collapsed
            };

            emailPanel.Children.Add(emailLabel);
            emailPanel.Children.Add(_authorEmailTextBox);
            emailPanel.Children.Add(_authorEmailError);
            Grid.SetRow(emailPanel, 3);
            contentGrid.Children.Add(emailPanel);

            // 提示訊息
            var infoPanel = new Border
            {
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 16),
                CornerRadius = new CornerRadius(4)
            };
            
            var infoStack = new StackPanel { Spacing = 4 };
            var infoTitle = new TextBlock
            {
                Text = "溫馨提示",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14
            };
            var infoText1 = new TextBlock
            {
                Text = "? 這些資訊將保存在本機，用於Git提交記錄",
                FontSize = 12
            };
            var infoText2 = new TextBlock
            {
                Text = "? 您可以稍後在設定中修改這些資訊",
                FontSize = 12
            };
            var infoText3 = new TextBlock
            {
                Text = "? 所有欄位都是必填的",
                FontSize = 12
            };

            infoStack.Children.Add(infoTitle);
            infoStack.Children.Add(infoText1);
            infoStack.Children.Add(infoText2);
            infoStack.Children.Add(infoText3);
            infoPanel.Child = infoStack;
            Grid.SetRow(infoPanel, 4);
            contentGrid.Children.Add(infoPanel);

            // 按鈕區域
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 8
            };

            var cancelButton = new Button
            {
                Content = "取消",
                MinWidth = 80
            };
            cancelButton.Click += CancelButton_Click;

            _saveButton = new Button
            {
                Content = "確定",
                MinWidth = 80,
                IsEnabled = false
            };
            _saveButton.Click += SaveButton_Click;

            buttonPanel.Children.Add(cancelButton);
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowExitConfirmation();
        }

        private async void ShowExitConfirmation()
        {
            var dialog = new ContentDialog
            {
                Title = "確認退出",
                Content = "您尚未完成初始設定。\n\n沒有作者資訊將無法使用Git功能。\n確定要退出應用程式嗎？",
                PrimaryButtonText = "重新設定",
                SecondaryButtonText = "退出應用程式",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Secondary)
            {
                // 用戶選擇退出
                Application.Current.Exit();
            }
            // 如果選擇重新設定，什麼都不做，保持窗口開啟
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