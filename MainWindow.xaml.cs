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
            
            // �ϥΥΤ�]�w�Ы�ViewModel
            ViewModel = new MainViewModel(userSettings);
            
            // �]�w���f���D
            this.Title = $"GitDWG - {userSettings.AuthorName}";
        }

        private async void OpenDrawingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filePath)
            {
                await ViewModel.OpenDrawingAsync(filePath);
            }
        }

        // �M�׿��ƥ�B�z
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
                        Title = "����",
                        Content = "�|����ܦ��Ī��M�׸�Ƨ�",
                        CloseButtonText = "�T�w",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "���~",
                    Content = $"�L�k�}�ұM�׸�Ƨ�: {ex.Message}",
                    CloseButtonText = "�T�w",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        // ���տ��ƥ�B�z
        private async void TestAutoCADConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // �o�̥i�H�K�[����AutoCAD�s�����޿�
                var dialog = new ContentDialog
                {
                    Title = "AutoCAD�s������",
                    Content = "���b����AutoCAD�s��...\n\n�o�ӥ\��N�b���Ӫ�������{�C",
                    CloseButtonText = "�T�w",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "���ե���",
                    Content = $"AutoCAD�s�����ե���: {ex.Message}",
                    CloseButtonText = "�T�w",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void TestDrawingOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�ϭ��}�Ҵ���",
                Content = "�o�ӥ\��N���\�z���չϭ��ɮת��}�ҥ\��C\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ���R���ƥ�B�z
        private async void AnalyzeCommitTrends_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�����Ͷդ��R",
                Content = "�o�ӥ\��N���R�z������Ҧ��M�ͶաC\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void AnalyzeFileChanges_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�ɮ��ܧ���R",
                Content = "�o�ӥ\��N���R�ɮ��ܧ��W�v�M�Ҧ��C\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        // �u����ƥ�B�z
        private async void DrawingCompareTools_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�ϭ�����u��",
                Content = "�o�ӥ\��N���Ѷi�����ϭ�����u��C\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void BatchProcessTools_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�妸�B�z�u��",
                Content = "�o�ӥ\��N���ѧ妸�B�z�h�ӹϭ��ɮת���O�C\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        // �����Ҳտ��ƥ�B�z
        private async void ManageExtensions_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�޲z�����Ҳ�",
                Content = "�o�ӥ\��N���\�z�޲z�w�w�˪������ҲաC\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void InstallExtensions_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�w�˩����Ҳ�",
                Content = "�o�ӥ\��N���\�z�w�˷s�������ҲաC\n\n�N�b���Ӫ�������{�C",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        // �������ƥ�B�z
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
                    Title = "���~",
                    Content = $"�L�k�إ߷s����: {ex.Message}",
                    CloseButtonText = "�T�w",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // �������ƥ�B�z
        private async void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "GitDWG �ϥλ���",
                Content = "GitDWG �O�@�ӱM��AutoCAD�ϭ��ɮ׳]�p��Git��������u��C\n\n" +
                         "�D�n�\��G\n" +
                         "? Git��������\n" +
                         "? AutoCAD�ϭ����\n" +
                         "? ���z�ɮ׺޲z\n" +
                         "? �h�Τ��@\n\n" +
                         "�Բӻ����аѦҲ��~���ɡC",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ShowQuickStart_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�ֳt�J��",
                Content = "GitDWG �ֳt�J���B�J�G\n\n" +
                         "1. ��ܩΪ�l��Git�x�s�w\n" +
                         "2. �]�wAutoCAD���|\n" +
                         "3. �ק�CAD�ϭ��ɮ�\n" +
                         "4. �ϥΧֳt����\��\n" +
                         "5. �d�ݴ�����v�M�������\n\n" +
                         "��h�Բӻ����Ьd�ݧ�����ɡC",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ShowKeyboardShortcuts_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "��L�ֱ���",
                Content = "�`�Χֱ���G\n\n" +
                         "Ctrl+O - �}���x�s�w\n" +
                         "Ctrl+N - ��l���x�s�w\n" +
                         "F5 - ���s��z\n" +
                         "Ctrl+Shift+F5 - �j��s��z\n" +
                         "Ctrl+Enter - �ֳt����\n" +
                         "Ctrl+A - �Ȧs�Ҧ�\n" +
                         "Ctrl+Shift+B - �إ߷s����\n" +
                         "Ctrl+H - �d�ݴ�����v\n" +
                         "Ctrl+P - �ﶵ�]�w\n" +
                         "F1 - ����",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�ˬd��s",
                Content = "�ثe�����GGitDWG v1.0.0\n\n" +
                         "���b�ˬd��s...\n\n" +
                         "�z�ثe�ϥΪ��O�̷s�����I",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "���� GitDWG",
                Content = "GitDWG v1.0.0\n\n" +
                         "�M��AutoCAD�]�p��Git��������u��\n\n" +
                         "�}�o�ζ��GGitDWG Development Team\n" +
                         "�޳N�䴩�Gsupport@gitdwg.com\n\n" +
                         "? 2024 GitDWG. All rights reserved.\n\n" +
                         "�P�±z�ϥ� GitDWG�I",
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
