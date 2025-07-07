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

        // ���
        private TextBox? _authorNameTextBox;
        private TextBox? _authorEmailTextBox;
        private TextBlock? _authorNameError;
        private TextBlock? _authorEmailError;
        private Button? _saveButton;

        public InitialSetupWindow()
        {
            _settingsService = new UserSettingsService();
            
            // �]�w���f�ݩ�
            this.Title = "GitDWG - ��l�]�w";
            
            // �Ы�UI
            CreateUI();
            
            // �q�\���f�����ƥ�
            this.Closed += Window_Closed;

            // �]�w�I��
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

            // ���J�{���]�w
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

            // ���D
            var titleBlock = new TextBlock
            {
                Text = "�w��ϥ� GitDWG",
                FontSize = 24,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(titleBlock, 0);
            contentGrid.Children.Add(titleBlock);

            var subtitleBlock = new TextBlock
            {
                Text = "�г]�w�z���@�̸�T�A�o�N�Ω�Git����O���C",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 24),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(subtitleBlock, 1);
            contentGrid.Children.Add(subtitleBlock);

            // �@�̩m�W
            var namePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var nameLabel = new TextBlock
            {
                Text = "�@�̩m�W *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            _authorNameTextBox = new TextBox
            {
                PlaceholderText = "�п�J�z���m�W"
            };
            _authorNameTextBox.TextChanged += ValidateInput;

            _authorNameError = new TextBlock
            {
                Text = "�п�J�@�̩m�W",
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

            // �@�̫H�c
            var emailPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var emailLabel = new TextBlock
            {
                Text = "�@�̫H�c *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            _authorEmailTextBox = new TextBox
            {
                PlaceholderText = "�п�J�z��Email�a�}"
            };
            _authorEmailTextBox.TextChanged += ValidateInput;

            _authorEmailError = new TextBlock
            {
                Text = "�п�J���Ī�Email�a�}",
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

            // ���ܰT��
            var infoPanel = new Border
            {
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 16),
                CornerRadius = new CornerRadius(4)
            };
            
            var infoStack = new StackPanel { Spacing = 4 };
            var infoTitle = new TextBlock
            {
                Text = "���ɴ���",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14
            };
            var infoText1 = new TextBlock
            {
                Text = "? �o�Ǹ�T�N�O�s�b�����A�Ω�Git����O��",
                FontSize = 12
            };
            var infoText2 = new TextBlock
            {
                Text = "? �z�i�H�y��b�]�w���ק�o�Ǹ�T",
                FontSize = 12
            };
            var infoText3 = new TextBlock
            {
                Text = "? �Ҧ���쳣�O����",
                FontSize = 12
            };

            infoStack.Children.Add(infoTitle);
            infoStack.Children.Add(infoText1);
            infoStack.Children.Add(infoText2);
            infoStack.Children.Add(infoText3);
            infoPanel.Child = infoStack;
            Grid.SetRow(infoPanel, 4);
            contentGrid.Children.Add(infoPanel);

            // ���s�ϰ�
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 8
            };

            var cancelButton = new Button
            {
                Content = "����",
                MinWidth = 80
            };
            cancelButton.Click += CancelButton_Click;

            _saveButton = new Button
            {
                Content = "�T�w",
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
                // �p�G�S���{���]�w�A�ϥιw�]��
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

            // ���ҧ@�̩m�W
            if (string.IsNullOrWhiteSpace(_authorNameTextBox.Text))
            {
                _authorNameError.Visibility = Visibility.Visible;
                _authorNameError.Text = "�п�J�@�̩m�W";
                isValid = false;
            }
            else if (_authorNameTextBox.Text.Trim().Length < 2)
            {
                _authorNameError.Visibility = Visibility.Visible;
                _authorNameError.Text = "�@�̩m�W�ܤֻݭn2�Ӧr��";
                isValid = false;
            }
            else
            {
                _authorNameError.Visibility = Visibility.Collapsed;
            }

            // ����Email
            if (string.IsNullOrWhiteSpace(_authorEmailTextBox.Text))
            {
                _authorEmailError.Visibility = Visibility.Visible;
                _authorEmailError.Text = "�п�JEmail�a�}";
                isValid = false;
            }
            else if (!IsValidEmail(_authorEmailTextBox.Text))
            {
                _authorEmailError.Visibility = Visibility.Visible;
                _authorEmailError.Text = "�п�J���Ī�Email�a�}";
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
                Title = "�T�{�h�X",
                Content = "�z�|��������l�]�w�C\n\n�S���@�̸�T�N�L�k�ϥ�Git�\��C\n�T�w�n�h�X���ε{���ܡH",
                PrimaryButtonText = "���s�]�w",
                SecondaryButtonText = "�h�X���ε{��",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Secondary)
            {
                // �Τ��ܰh�X
                Application.Current.Exit();
            }
            // �p�G��ܭ��s�]�w�A���򳣤����A�O�����f�}��
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
                // �p�G�]�w�������B���f�Q�����A�h�X���ε{��
                Application.Current.Exit();
            }
        }
    }
}