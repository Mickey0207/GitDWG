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
    }
}
