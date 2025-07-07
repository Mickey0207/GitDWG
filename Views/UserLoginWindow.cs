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

        // ���
        private ComboBox? _userComboBox;
        private PasswordBox? _passwordBox;
        private TextBlock? _errorMessage;
        private Button? _loginButton;
        private Button? _addUserButton;

        public UserLoginWindow()
        {
            _authService = new UserAuthenticationService();
            _settingsService = new UserSettingsService();
            
            // �]�w���f�ݩ�
            this.Title = "GitDWG - �Τ�n�J";
            
            // �Ы�UI
            CreateUI();
            
            // �q�\���f�����ƥ�
            this.Closed += Window_Closed;

            // �]�w�I��
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            
            this.Activate();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 17, 24, 39)) // �`��D�I��
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
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // ���D�M�ϼ�
            var titlePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 24)
            };
            
            var titleBlock = new TextBlock
            {
                Text = "GitDWG �Τ�n�J",
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)) // �L���r
            };
            
            titlePanel.Children.Add(titleBlock);
            Grid.SetRow(titlePanel, 0);
            contentGrid.Children.Add(titlePanel);

            var subtitleBlock = new TextBlock
            {
                Text = "�п�ܱz���Τ�W�٨ÿ�J�K�X",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 32),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 148, 163, 184)) // �Ǧ��r
            };
            Grid.SetRow(subtitleBlock, 1);
            contentGrid.Children.Add(subtitleBlock);

            // �Τ���
            var userPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var userLabel = new TextBlock
            {
                Text = "��ܥΤ�",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // �L�Ǧ����
            };
            
            _userComboBox = new ComboBox
            {
                PlaceholderText = "�п�ܥΤ�",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // �`��I��
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // �զ��r
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // �`�����
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            
            // ���J�Τ�C��
            LoadUsers();

            userPanel.Children.Add(userLabel);
            userPanel.Children.Add(_userComboBox);
            Grid.SetRow(userPanel, 2);
            contentGrid.Children.Add(userPanel);

            // �K�X��J
            var passwordPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var passwordLabel = new TextBlock
            {
                Text = "��J�K�X",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 226, 232, 240)) // �L�Ǧ����
            };
            
            _passwordBox = new PasswordBox
            {
                PlaceholderText = "�п�J�K�X",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)), // �`��I��
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)), // �զ��r
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99)), // �`�����
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            _passwordBox.KeyDown += PasswordBox_KeyDown;

            passwordPanel.Children.Add(passwordLabel);
            passwordPanel.Children.Add(_passwordBox);
            Grid.SetRow(passwordPanel, 3);
            contentGrid.Children.Add(passwordPanel);

            // ���~�T��
            _errorMessage = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // �L������~
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 16),
                Visibility = Visibility.Collapsed,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(_errorMessage, 4);
            contentGrid.Children.Add(_errorMessage);

            // ���s�ϰ�
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Margin = new Thickness(0, 16, 0, 0)
            };

            _loginButton = new Button
            {
                Content = "�n�J",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 44,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)), // ���D�n���s
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // �զ��r
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(10)
            };
            _loginButton.Click += LoginButton_Click;

            _addUserButton = new Button
            {
                Content = "�s�W�Τ�",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 99, 102, 241)), // �Ŧ⦸�n���s
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // �զ��r
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(8)
            };
            _addUserButton.Click += AddUserButton_Click;

            var exitButton = new Button
            {
                Content = "�h�X���ε{��",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)), // �L����M�I���s
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255)), // �զ��r
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(8)
            };
            exitButton.Click += ExitButton_Click;

            buttonPanel.Children.Add(_loginButton);
            buttonPanel.Children.Add(_addUserButton);
            buttonPanel.Children.Add(exitButton);
            Grid.SetRow(buttonPanel, 5);
            contentGrid.Children.Add(buttonPanel);

            // ������r - �`��]�p
            var infoPanel = new Border
            {
                Padding = new Thickness(16),
                Margin = new Thickness(0, 24, 0, 0),
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
                Text = "�ϥλ���",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 209, 250, 229)) // �L�����D
            };
            var infoText1 = new TextBlock
            {
                Text = "? ��ܱz���Τ�W�٨ÿ�J�����K�X",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // �L����r
            };
            var infoText2 = new TextBlock
            {
                Text = "? �s�W�Τ�ݭn�޲z���K�X���v",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // �L����r
            };
            var infoText3 = new TextBlock
            {
                Text = "? �n�J���\��N�i�JGit��������t��",
                FontSize = 12,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 187, 247, 208)) // �L����r
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
                ShowError("�п�ܥΤ�");
                return;
            }

            if (string.IsNullOrEmpty(_passwordBox?.Password))
            {
                ShowError("�п�J�K�X");
                return;
            }

            var userName = _userComboBox.SelectedItem.ToString();
            var password = _passwordBox.Password;

            if (_authService.AuthenticateUser(userName!, password))
            {
                // �n�J���\�A�Ы�UserSettings
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
                ShowError("�Τ�W�٩αK�X���~�A�Э��s��J");
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
                Title = "�s�W�Τ�",
                PrimaryButtonText = "�s�W",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark // �`���ܮ�
            };

            var panel = new StackPanel { Spacing = 16 };
            
            var adminPasswordBox = new PasswordBox
            {
                Header = "�޲z���K�X",
                PlaceholderText = "�п�J�޲z���K�X",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99))
            };
            
            var newUserNameBox = new TextBox
            {
                Header = "�s�Τ�W��",
                PlaceholderText = "�п�J�s�Τ�W��",
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 51, 65, 85)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 75, 85, 99))
            };
            
            var newUserPasswordBox = new PasswordBox
            {
                Header = "�s�Τ�K�X",
                PlaceholderText = "�п�J�s�Τ�K�X",
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
                    ShowError("�޲z���K�X���~");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newUserNameBox.Text))
                {
                    ShowError("�п�J�Τ�W��");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newUserPasswordBox.Password))
                {
                    ShowError("�п�J�Τ�K�X");
                    return;
                }

                if (_authService.AddUser(newUserNameBox.Text.Trim(), newUserPasswordBox.Password))
                {
                    LoadUsers(); // ���s���J�Τ�C��
                    ShowSuccess($"�Τ� '{newUserNameBox.Text.Trim()}' �s�W���\�I");
                }
                else
                {
                    ShowError("�Τ�w�s�b�ηs�W����");
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
                _errorMessage.Text = "���~: " + message;
                _errorMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113)); // �L����
                _errorMessage.Visibility = Visibility.Visible;
            }
        }

        private void ShowSuccess(string message)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Text = "���\: " + message;
                _errorMessage.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 16, 185, 129)); // ���
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
                // �p�G�n�J�����\�B���f�Q�����A�h�X���ε{��
                Application.Current.Exit();
            }
        }
    }
}