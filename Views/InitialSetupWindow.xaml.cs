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
            var mainGrid = new Grid
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 17, 24, 39)) // �`��I��
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
                    Windows.UI.Color.FromArgb(255, 30, 41, 59)), // �`���ŭI��
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(80, 59, 130, 246)), // �Ŧ����
                BorderThickness = new Thickness(1)
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
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)) // �L���r
            };
            Grid.SetRow(titleBlock, 0);
            contentGrid.Children.Add(titleBlock);

            var subtitleBlock = new TextBlock
            {
                Text = "�г]�w�z���@�̸�T�A�o�N�Ω�Git����O���C",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 32),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 148, 163, 184)) // �Ǧ��r
            };
            Grid.SetRow(subtitleBlock, 1);
            contentGrid.Children.Add(subtitleBlock);

            // �@�̩m�W
            var namePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var nameLabel = new TextBlock
            {
                Text = "�@�̩m�W *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // �L�Ǧ����
            };
            _authorNameTextBox = new TextBox
            {
                PlaceholderText = "�п�J�z���m�W",
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // �`���J�حI��
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // �զ��r
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // �`�����
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            _authorNameTextBox.TextChanged += ValidateInput;

            _authorNameError = new TextBlock
            {
                Text = "�п�J�@�̩m�W",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // �L������~����
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Visibility = Visibility.Collapsed
            };

            namePanel.Children.Add(nameLabel);
            namePanel.Children.Add(_authorNameTextBox);
            namePanel.Children.Add(_authorNameError);
            Grid.SetRow(namePanel, 2);
            contentGrid.Children.Add(namePanel);

            // �@�̫H�c
            var emailPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var emailLabel = new TextBlock
            {
                Text = "�@�̫H�c *",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // �L�Ǧ����
            };
            _authorEmailTextBox = new TextBox
            {
                PlaceholderText = "�п�J�z��Email�a�}",
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // �`���J�حI��
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // �զ��r
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // �`�����
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            _authorEmailTextBox.TextChanged += ValidateInput;

            _authorEmailError = new TextBlock
            {
                Text = "�п�J���Ī�Email�a�}",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // �L������~����
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Visibility = Visibility.Collapsed
            };

            emailPanel.Children.Add(emailLabel);
            emailPanel.Children.Add(_authorEmailTextBox);
            emailPanel.Children.Add(_authorEmailError);
            Grid.SetRow(emailPanel, 3);
            contentGrid.Children.Add(emailPanel);

            // ���ܰT�� - �`��]�p
            var infoPanel = new Border
            {
                Padding = new Thickness(16),
                Margin = new Thickness(0, 8, 0, 24),
                CornerRadius = new CornerRadius(8),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(40, 16, 185, 129)), // �b�z�����I��
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(60, 16, 185, 129)), // ������
                BorderThickness = new Thickness(1)
            };
            
            var infoStack = new StackPanel { Spacing = 6 };
            var infoTitle = new TextBlock
            {
                Text = "�]�w����",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 209, 250, 229)) // �L�����D
            };
            var infoText1 = new TextBlock
            {
                Text = "? �o�Ǹ�T�N�O�s�b�����A�Ω�Git����O��",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // �L����r
            };
            var infoText2 = new TextBlock
            {
                Text = "? �z�i�H�y��b�]�w���ק�o�Ǹ�T",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // �L����r
            };
            var infoText3 = new TextBlock
            {
                Text = "? �Ҧ���쳣�O���񪺡A������Y�i�}�l�ϥ�",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // �L����r
            };

            infoStack.Children.Add(infoTitle);
            infoStack.Children.Add(infoText1);
            infoStack.Children.Add(infoText2);
            infoStack.Children.Add(infoText3);
            infoPanel.Child = infoStack;
            Grid.SetRow(infoPanel, 4);
            contentGrid.Children.Add(infoPanel);

            // ���s�ϰ� - �u�O�d�T�w���s
            var buttonPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 16, 0, 0)
            };

            _saveButton = new Button
            {
                Content = "�����]�w",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 44,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)), // ���D�n���s
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // �զ��r
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10),
                IsEnabled = false
            };
            _saveButton.Click += SaveButton_Click;

            // �K�[Enter��䴩
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
            
            // �ʺA��s���s�˦�
            if (isValid)
            {
                _saveButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)); // �ҥήɪ����
            }
            else
            {
                _saveButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)); // �T�ήɪ��Ǧ�
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
                // �p�G�]�w�������B���f�Q�����A�h�X���ε{��
                Application.Current.Exit();
            }
        }
    }
}