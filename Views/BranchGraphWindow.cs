using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using GitDWG.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using Windows.UI;

namespace GitDWG.Views
{
    public sealed class BranchGraphWindow : Window
    {
        private readonly GitService _gitService;
        private Canvas _graphCanvas = null!;
        private ScrollViewer _scrollViewer = null!;
        private StackPanel _branchListPanel = null!;
        private StackPanel _commitDetailsPanel = null!;
        private List<BranchGraphNode> _graphNodes;
        private List<CommitInfo> _commits;
        private List<string> _branches;
        private string? _selectedBranch;

        public BranchGraphWindow(GitService gitService)
        {
            _gitService = gitService;
            _graphNodes = new List<BranchGraphNode>();
            _commits = new List<CommitInfo>();
            _branches = new List<string>();

            this.Title = "GitDWG - ����ϧκ޲z��";
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));

            CreateUI();
            LoadBranchData();

            this.Activate();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 17, 24, 39))
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(380) });

            CreateBranchListPanel(mainGrid);
            CreateGraphArea(mainGrid);
            CreateDetailsPanel(mainGrid);

            this.Content = mainGrid;
        }

        private void CreateBranchListPanel(Grid mainGrid)
        {
            var leftBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)),
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(20)
            };

            var leftPanel = new StackPanel();

            var branchTitle = new TextBlock
            {
                Text = "����޲z",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249))
            };
            leftPanel.Children.Add(branchTitle);

            var branchActionsPanel = new StackPanel { Spacing = 12, Margin = new Thickness(0, 0, 0, 24) };

            var createBranchButton = new Button
            {
                Content = "�إ߷s����",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 44,
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Background = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(0)
            };
            createBranchButton.Click += CreateBranchButton_Click;
            branchActionsPanel.Children.Add(createBranchButton);

            var mergeBranchButton = new Button
            {
                Content = "�X�֤���",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(0),
                IsEnabled = false
            };
            mergeBranchButton.Click += MergeBranchButton_Click;
            branchActionsPanel.Children.Add(mergeBranchButton);

            var deleteBranchButton = new Button
            {
                Content = "�R������",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromArgb(255, 248, 113, 113)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(0),
                IsEnabled = false
            };
            deleteBranchButton.Click += DeleteBranchButton_Click;
            branchActionsPanel.Children.Add(deleteBranchButton);

            var renameBranchButton = new Button
            {
                Content = "���R�W����",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromArgb(255, 245, 158, 11)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(0),
                IsEnabled = false
            };
            renameBranchButton.Click += RenameBranchButton_Click;
            branchActionsPanel.Children.Add(renameBranchButton);

            leftPanel.Children.Add(branchActionsPanel);

            var branchListTitle = new TextBlock
            {
                Text = "�Ҧ�����",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            leftPanel.Children.Add(branchListTitle);

            _branchListPanel = new StackPanel();
            var branchScrollViewer = new ScrollViewer
            {
                Content = _branchListPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 400
            };
            leftPanel.Children.Add(branchScrollViewer);

            leftBorder.Child = leftPanel;
            Grid.SetColumn(leftBorder, 0);
            mainGrid.Children.Add(leftBorder);
        }

        private void CreateGraphArea(Grid mainGrid)
        {
            var centerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 30, 42)),
                Padding = new Thickness(24)
            };

            var centerPanel = new StackPanel();

            var headerPanel = new Grid();
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var graphTitle = new TextBlock
            {
                Text = "����ϧε���",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249))
            };
            Grid.SetColumn(graphTitle, 0);
            headerPanel.Children.Add(graphTitle);

            var toolBar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            var refreshButton = new Button
            {
                Content = "���s��z",
                FontSize = 13,
                Padding = new Thickness(16, 8, 16, 8),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)),
                BorderThickness = new Thickness(0)
            };
            refreshButton.Click += RefreshButton_Click;

            var fitToWindowButton = new Button
            {
                Content = "�A�X����",
                FontSize = 13,
                Padding = new Thickness(16, 8, 16, 8),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)),
                BorderThickness = new Thickness(0)
            };
            fitToWindowButton.Click += FitToWindowButton_Click;

            var exportButton = new Button
            {
                Content = "�ץX�Ϥ�",
                FontSize = 13,
                Padding = new Thickness(16, 8, 16, 8),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromArgb(255, 99, 102, 241)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                BorderThickness = new Thickness(0)
            };
            exportButton.Click += ExportButton_Click;

            toolBar.Children.Add(refreshButton);
            toolBar.Children.Add(fitToWindowButton);
            toolBar.Children.Add(exportButton);
            Grid.SetColumn(toolBar, 1);
            headerPanel.Children.Add(toolBar);

            centerPanel.Children.Add(headerPanel);
            centerPanel.Children.Add(new Border { Height = 24 });

            _graphCanvas = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)),
                MinHeight = 700,
                MinWidth = 800
            };

            _scrollViewer = new ScrollViewer
            {
                Content = _graphCanvas,
                ZoomMode = ZoomMode.Enabled,
                HorizontalScrollMode = ScrollMode.Enabled,
                VerticalScrollMode = ScrollMode.Enabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 30, 42)),
                Height = 600
            };

            centerPanel.Children.Add(_scrollViewer);
            centerBorder.Child = centerPanel;
            Grid.SetColumn(centerBorder, 1);
            mainGrid.Children.Add(centerBorder);
        }

        private void CreateDetailsPanel(Grid mainGrid)
        {
            var rightBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)),
                BorderThickness = new Thickness(1, 0, 0, 0),
                Padding = new Thickness(20)
            };

            var rightPanel = new StackPanel();

            var detailsTitle = new TextBlock
            {
                Text = "�ԲӸ�T",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249))
            };
            rightPanel.Children.Add(detailsTitle);

            var branchInfoPanel = CreateBranchInfoPanel();
            rightPanel.Children.Add(branchInfoPanel);

            _commitDetailsPanel = new StackPanel();

            var noSelectionText = new TextBlock
            {
                Text = "�I������`�I�ο�ܤ���H�d�ݸԲӸ�T",
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0)
            };
            _commitDetailsPanel.Children.Add(noSelectionText);

            rightPanel.Children.Add(_commitDetailsPanel);
            rightBorder.Child = rightPanel;
            Grid.SetColumn(rightBorder, 2);
            mainGrid.Children.Add(rightBorder);
        }

        private Border CreateBranchInfoPanel()
        {
            var infoBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 20),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)),
                BorderThickness = new Thickness(1)
            };

            var infoPanel = new StackPanel { Spacing = 12 };

            var infoTitle = new TextBlock
            {
                Text = "����έp",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            infoPanel.Children.Add(infoTitle);

            var branchCountText = new TextBlock
            {
                Text = $"�`����ơG{_branches.Count}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            infoPanel.Children.Add(branchCountText);

            var currentBranchText = new TextBlock
            {
                Text = $"��e����G{_gitService.GetCurrentBranch()}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129))
            };
            infoPanel.Children.Add(currentBranchText);

            var commitCountText = new TextBlock
            {
                Text = $"����O���G{_commits.Count}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            infoPanel.Children.Add(commitCountText);

            infoBorder.Child = infoPanel;
            return infoBorder;
        }

        private void LoadBranchData()
        {
            try
            {
                if (!_gitService.IsRepositoryValid())
                {
                    ShowMessage("Git�x�s�w����l��");
                    return;
                }

                _branches = _gitService.GetBranches().ToList();
                UpdateBranchList();

                _commits = _gitService.GetCommitHistory(50).ToList();
                CreateBranchGraph();

                UpdateBranchInfoPanel();
            }
            catch (Exception ex)
            {
                ShowMessage($"���J�����ƥ���: {ex.Message}");
            }
        }

        private void UpdateBranchInfoPanel()
        {
            var rightBorder = (Border)((Grid)this.Content).Children[2];
            var rightPanel = (StackPanel)rightBorder.Child;

            if (rightPanel.Children.Count > 1 && rightPanel.Children[1] is Border)
            {
                rightPanel.Children.RemoveAt(1);
            }
            rightPanel.Children.Insert(1, CreateBranchInfoPanel());
        }

        private void UpdateBranchList()
        {
            _branchListPanel.Children.Clear();
            var currentBranch = _gitService.GetCurrentBranch();

            foreach (var branch in _branches)
            {
                var branchItem = CreateBranchListItem(branch, currentBranch);
                _branchListPanel.Children.Add(branchItem);
            }
        }

        private Border CreateBranchListItem(string branch, string currentBranch)
        {
            var isCurrentBranch = branch == currentBranch;
            var branchItem = new Border
            {
                Background = isCurrentBranch ?
                    new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)) :
                    new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 4, 0, 4)
            };

            var branchPanel = new Grid();
            branchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            branchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            branchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var branchIcon = new TextBlock
            {
                Text = isCurrentBranch ? "��" : "��",
                FontSize = 14,
                Foreground = isCurrentBranch ?
                    new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) :
                    GetBranchColor(GetBranchTypeIndex(branch)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(branchIcon, 0);

            var branchName = new TextBlock
            {
                Text = branch,
                FontSize = 14,
                FontWeight = isCurrentBranch ?
                    Microsoft.UI.Text.FontWeights.Bold :
                    Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = isCurrentBranch ?
                    new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) :
                    new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(branchName, 1);

            var branchTypeLabel = new Border
            {
                Background = GetBranchColor(GetBranchTypeIndex(branch)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Opacity = 0.8
            };

            var branchTypeText = new TextBlock
            {
                Text = GetBranchTypeText(branch),
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };
            branchTypeLabel.Child = branchTypeText;
            Grid.SetColumn(branchTypeLabel, 2);

            branchPanel.Children.Add(branchIcon);
            branchPanel.Children.Add(branchName);
            branchPanel.Children.Add(branchTypeLabel);
            branchItem.Child = branchPanel;

            branchItem.PointerPressed += (s, e) => OnBranchSelected(branch);

            return branchItem;
        }

        private int GetBranchTypeIndex(string branchName)
        {
            var lowerName = branchName.ToLower();
            if (lowerName.Contains("main") || lowerName.Contains("master")) return 0;
            if (lowerName.Contains("feature") || lowerName.Contains("feat")) return 1;
            if (lowerName.Contains("fix") || lowerName.Contains("bug")) return 2;
            if (lowerName.Contains("hotfix")) return 3;
            if (lowerName.Contains("develop") || lowerName.Contains("dev")) return 4;
            if (lowerName.Contains("release")) return 5;
            return 0;
        }

        private string GetBranchTypeText(string branchName)
        {
            var index = GetBranchTypeIndex(branchName);
            return index switch
            {
                0 => "MAIN",
                1 => "FEAT",
                2 => "FIX",
                3 => "HOT",
                4 => "DEV",
                5 => "REL",
                _ => "MAIN"
            };
        }

        private async void MergeBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedBranch))
            {
                ShowMessage("�Х���ܭn�X�֪�����");
                return;
            }

            var currentBranch = _gitService.GetCurrentBranch();
            if (_selectedBranch == currentBranch)
            {
                ShowMessage("�L�k�X�ַ�e�����ۤv");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "�X�֤���",
                Content = $"�T�w�n�N���� '{_selectedBranch}' �X�֨��e���� '{currentBranch}' �ܡH\n\n�o�Ӿާ@�L�k�M�P�C",
                PrimaryButtonText = "�X��",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _gitService.MergeBranch(_selectedBranch);
                    LoadBranchData();
                    ShowMessage($"�w���\�X�֤��� '{_selectedBranch}' �� '{currentBranch}'");
                }
                catch (Exception ex)
                {
                    ShowMessage($"�X�֤��䥢��: {ex.Message}");
                }
            }
        }

        private async void DeleteBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedBranch))
            {
                ShowMessage("�Х���ܭn�R��������");
                return;
            }

            var currentBranch = _gitService.GetCurrentBranch();
            if (_selectedBranch == currentBranch)
            {
                ShowMessage("�L�k�R����e����");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "�R������",
                Content = $"�T�w�n�R������ '{_selectedBranch}' �ܡH\n\n�o�Ӿާ@�L�k�M�P�I",
                PrimaryButtonText = "�R��",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _gitService.DeleteBranch(_selectedBranch);
                    _selectedBranch = null;
                    LoadBranchData();
                    UpdateBranchActionButtons();
                    ShowMessage($"�w���\�R������ '{_selectedBranch}'");
                }
                catch (Exception ex)
                {
                    ShowMessage($"�R�����䥢��: {ex.Message}");
                }
            }
        }

        private async void RenameBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedBranch))
            {
                ShowMessage("�Х���ܭn���R�W������");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "���R�W����",
                PrimaryButtonText = "���R�W",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            var panel = new StackPanel { Spacing = 12 };

            var currentNameLabel = new TextBlock
            {
                Text = $"��e�W�١G{_selectedBranch}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };

            var newNameLabel = new TextBlock
            {
                Text = "�s�W�١G",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };

            var textBox = new TextBox
            {
                Text = _selectedBranch,
                Margin = new Thickness(0, 4, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99))
            };

            panel.Children.Add(currentNameLabel);
            panel.Children.Add(newNameLabel);
            panel.Children.Add(textBox);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                var newName = textBox.Text.Trim();
                if (newName != _selectedBranch)
                {
                    try
                    {
                        _gitService.RenameBranch(_selectedBranch, newName);
                        _selectedBranch = newName;
                        LoadBranchData();
                        ShowMessage($"�w���\���R�W���䬰 '{newName}'");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"���R�W���䥢��: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateBranchActionButtons()
        {
            var leftBorder = (Border)((Grid)this.Content).Children[0];
            var leftPanel = (StackPanel)leftBorder.Child;
            var branchActionsPanel = (StackPanel)leftPanel.Children[1];

            var hasBranchSelected = !string.IsNullOrEmpty(_selectedBranch);
            var currentBranch = _gitService.GetCurrentBranch();
            var isCurrentBranch = _selectedBranch == currentBranch;

            ((Button)branchActionsPanel.Children[1]).IsEnabled = hasBranchSelected && !isCurrentBranch;
            ((Button)branchActionsPanel.Children[2]).IsEnabled = hasBranchSelected && !isCurrentBranch;
            ((Button)branchActionsPanel.Children[3]).IsEnabled = hasBranchSelected;
        }

        private void OnBranchSelected(string branchName)
        {
            _selectedBranch = branchName;
            UpdateBranchActionButtons();

            try
            {
                if (branchName != _gitService.GetCurrentBranch())
                {
                    ShowBranchSwitchDialog(branchName);
                }

                ShowBranchDetails(branchName);
            }
            catch (Exception ex)
            {
                ShowMessage($"���J�����T����: {ex.Message}");
            }
        }

        private void ShowBranchDetails(string branchName)
        {
            _commitDetailsPanel.Children.Clear();

            var namePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var nameLabel = new TextBlock
            {
                Text = "����W��:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var nameValue = new TextBlock
            {
                Text = branchName,
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = GetBranchColor(GetBranchTypeIndex(branchName))
            };
            namePanel.Children.Add(nameLabel);
            namePanel.Children.Add(nameValue);

            var typePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var typeLabel = new TextBlock
            {
                Text = "��������:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var typeValue = new TextBlock
            {
                Text = GetBranchTypeFullName(branchName),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            typePanel.Children.Add(typeLabel);
            typePanel.Children.Add(typeValue);

            var statusPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var statusLabel = new TextBlock
            {
                Text = "���A:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var isCurrentBranch = branchName == _gitService.GetCurrentBranch();
            var statusValue = new TextBlock
            {
                Text = isCurrentBranch ? "��e����" : "��L����",
                FontSize = 13,
                Foreground = isCurrentBranch ?
                    new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)) :
                    new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            statusPanel.Children.Add(statusLabel);
            statusPanel.Children.Add(statusValue);

            if (!isCurrentBranch)
            {
                var actionPanel = new StackPanel { Margin = new Thickness(0, 20, 0, 0), Spacing = 10 };

                var switchButton = new Button
                {
                    Content = "�����즹����",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    FontSize = 13,
                    Padding = new Thickness(12, 8, 12, 8),
                    Background = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)),
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    CornerRadius = new CornerRadius(6),
                    BorderThickness = new Thickness(0)
                };
                switchButton.Click += (s, e) => ShowBranchSwitchDialog(branchName);
                actionPanel.Children.Add(switchButton);

                _commitDetailsPanel.Children.Add(actionPanel);
            }

            _commitDetailsPanel.Children.Add(namePanel);
            _commitDetailsPanel.Children.Add(typePanel);
            _commitDetailsPanel.Children.Add(statusPanel);
        }

        private string GetBranchTypeFullName(string branchName)
        {
            var index = GetBranchTypeIndex(branchName);
            return index switch
            {
                0 => "�D���� (Main Branch)",
                1 => "�\����� (Feature Branch)",
                2 => "�״_���� (Bugfix Branch)",
                3 => "���״_���� (Hotfix Branch)",
                4 => "�}�o���� (Development Branch)",
                5 => "�o������ (Release Branch)",
                _ => "�D���� (Main Branch)"
            };
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("�ץX�Ϥ��\��}�o��...");
        }

        private void CreateBranchGraph()
        {
            _graphCanvas.Children.Clear();
            _graphNodes.Clear();

            if (!_commits.Any()) return;

            var branchCommits = AnalyzeBranchStructure();

            const double nodeSpacing = 80;
            const double branchSpacing = 120;
            const double nodeRadius = 10;
            const double startX = 80;
            const double startY = 50;

            int commitIndex = 0;
            foreach (var commit in _commits)
            {
                var branchIndex = GetBranchIndex(commit, branchCommits);

                var x = startX + branchIndex * branchSpacing;
                var y = startY + commitIndex * nodeSpacing;

                var node = CreateCommitNode(commit, x, y, nodeRadius, branchIndex);
                _graphNodes.Add(node);

                if (commitIndex > 0)
                {
                    DrawConnectionLines(commit, x, y, nodeRadius, branchCommits, startX, startY, nodeSpacing, branchSpacing);
                }

                commitIndex++;
            }

            DrawBranchLabels(branchCommits, startX, startY, branchSpacing);
            AdjustCanvasSize(startX, startY, nodeSpacing, branchSpacing, branchCommits.Count);
        }

        private Dictionary<string, List<CommitInfo>> AnalyzeBranchStructure()
        {
            var branchCommits = new Dictionary<string, List<CommitInfo>>();

            foreach (var branch in _branches)
            {
                branchCommits[branch] = new List<CommitInfo>();
            }

            var mainBranch = _branches.Contains("main") ? "main" :
                            _branches.Contains("master") ? "master" :
                            _branches.FirstOrDefault() ?? "main";

            foreach (var commit in _commits)
            {
                if (!branchCommits.ContainsKey(mainBranch))
                    branchCommits[mainBranch] = new List<CommitInfo>();
                branchCommits[mainBranch].Add(commit);
            }

            return branchCommits;
        }

        private int GetBranchIndex(CommitInfo commit, Dictionary<string, List<CommitInfo>> branchCommits)
        {
            if (commit.Message.ToLower().Contains("feature") || commit.Message.ToLower().Contains("feat"))
                return 1;
            else if (commit.Message.ToLower().Contains("fix") || commit.Message.ToLower().Contains("bug"))
                return 2;
            else if (commit.Message.ToLower().Contains("hotfix"))
                return 3;

            return 0;
        }

        private void DrawConnectionLines(CommitInfo commit, double x, double y, double nodeRadius,
            Dictionary<string, List<CommitInfo>> branchCommits, double startX, double startY,
            double nodeSpacing, double branchSpacing)
        {
            var currentIndex = _commits.IndexOf(commit);
            if (currentIndex <= 0) return;

            var previousCommit = _commits[currentIndex - 1];
            var prevBranchIndex = GetBranchIndex(previousCommit, branchCommits);
            var currentBranchIndex = GetBranchIndex(commit, branchCommits);

            var prevX = startX + prevBranchIndex * branchSpacing;
            var prevY = startY + (currentIndex - 1) * nodeSpacing;

            if (currentBranchIndex == prevBranchIndex)
            {
                var straightLine = new Line
                {
                    X1 = x,
                    Y1 = prevY + nodeRadius,
                    X2 = x,
                    Y2 = y - nodeRadius,
                    Stroke = GetBranchColor(currentBranchIndex),
                    StrokeThickness = 4
                };
                Canvas.SetZIndex(straightLine, 0);
                _graphCanvas.Children.Add(straightLine);
            }
            else
            {
                var path = new Microsoft.UI.Xaml.Shapes.Path
                {
                    Stroke = GetBranchColor(currentBranchIndex),
                    StrokeThickness = 3,
                    Data = CreateCurvedPath(prevX, prevY + nodeRadius, x, y - nodeRadius)
                };
                Canvas.SetZIndex(path, 0);
                _graphCanvas.Children.Add(path);
            }
        }

        private SolidColorBrush GetBranchColor(int branchIndex)
        {
            var colors = new[]
            {
                Windows.UI.Color.FromArgb(255, 59, 130, 246),
                Windows.UI.Color.FromArgb(255, 16, 185, 129),
                Windows.UI.Color.FromArgb(255, 245, 158, 11),
                Windows.UI.Color.FromArgb(255, 239, 68, 68),
                Windows.UI.Color.FromArgb(255, 168, 85, 247),
                Windows.UI.Color.FromArgb(255, 236, 72, 153),
            };

            return new SolidColorBrush(colors[branchIndex % colors.Length]);
        }

        private Microsoft.UI.Xaml.Media.Geometry CreateCurvedPath(double x1, double y1, double x2, double y2)
        {
            var pathGeometry = new Microsoft.UI.Xaml.Media.PathGeometry();
            var pathFigure = new Microsoft.UI.Xaml.Media.PathFigure
            {
                StartPoint = new Windows.Foundation.Point(x1, y1)
            };

            var bezierSegment = new Microsoft.UI.Xaml.Media.BezierSegment
            {
                Point1 = new Windows.Foundation.Point(x1, y1 + (y2 - y1) / 3),
                Point2 = new Windows.Foundation.Point(x2, y2 - (y2 - y1) / 3),
                Point3 = new Windows.Foundation.Point(x2, y2)
            };

            pathFigure.Segments.Add(bezierSegment);
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private void DrawBranchLabels(Dictionary<string, List<CommitInfo>> branchCommits,
            double startX, double startY, double branchSpacing)
        {
            var branchNames = new[] { "main", "feature", "bugfix", "hotfix", "develop", "release" };

            for (int i = 0; i < Math.Min(branchNames.Length, 6); i++)
            {
                var branchLabel = new Border
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 51, 65, 85)),
                    Padding = new Thickness(12, 6, 12, 6),
                    CornerRadius = new CornerRadius(16),
                    BorderBrush = GetBranchColor(i),
                    BorderThickness = new Thickness(2)
                };

                var labelText = new TextBlock
                {
                    Text = branchNames[i],
                    FontSize = 13,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = GetBranchColor(i)
                };

                branchLabel.Child = labelText;
                Canvas.SetLeft(branchLabel, startX + i * branchSpacing - 30);
                Canvas.SetTop(branchLabel, startY - 35);
                Canvas.SetZIndex(branchLabel, 3);

                _graphCanvas.Children.Add(branchLabel);
            }
        }

        private void AdjustCanvasSize(double startX, double startY, double nodeSpacing,
            double branchSpacing, int branchCount)
        {
            if (_commits.Any())
            {
                var maxY = startY + (_commits.Count - 1) * nodeSpacing + 100;
                var maxX = startX + branchCount * branchSpacing + 200;

                _graphCanvas.Height = Math.Max(700, maxY);
                _graphCanvas.Width = Math.Max(800, maxX);
            }
        }

        private BranchGraphNode CreateCommitNode(CommitInfo commit, double x, double y, double radius, int branchIndex)
        {
            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = GetBranchColor(branchIndex),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)),
                StrokeThickness = 3
            };
            Canvas.SetLeft(circle, x - radius);
            Canvas.SetTop(circle, y - radius);
            Canvas.SetZIndex(circle, 2);

            var messageBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 51, 65, 85)),
                Padding = new Thickness(10, 6, 10, 6),
                MaxWidth = 280,
                CornerRadius = new CornerRadius(8),
                BorderBrush = GetBranchColor(branchIndex),
                BorderThickness = new Thickness(1.5)
            };
            Canvas.SetLeft(messageBorder, x + radius + 16);
            Canvas.SetTop(messageBorder, y - 12);
            Canvas.SetZIndex(messageBorder, 1);

            var messageLabel = new TextBlock
            {
                Text = commit.Message.Length > 50 ?
                    commit.Message.Substring(0, 50) + "..." :
                    commit.Message,
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            messageBorder.Child = messageLabel;

            var shaBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(180, 75, 85, 99)),
                Padding = new Thickness(6, 3, 6, 3),
                CornerRadius = new CornerRadius(4),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(shaBorder, x + radius + 16);
            Canvas.SetTop(shaBorder, y + 8);
            Canvas.SetZIndex(shaBorder, 1);

            var shaLabel = new TextBlock
            {
                Text = commit.ShortSha,
                FontSize = 11,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = GetBranchColor(branchIndex)
            };
            shaBorder.Child = shaLabel;

            var authorDateLabel = new TextBlock
            {
                Text = $"{commit.Author} ? {commit.Date:MM/dd HH:mm}",
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)),
                MaxWidth = 250
            };
            Canvas.SetLeft(authorDateLabel, x + radius + 16);
            Canvas.SetTop(authorDateLabel, y + 28);
            Canvas.SetZIndex(authorDateLabel, 1);

            _graphCanvas.Children.Add(circle);
            _graphCanvas.Children.Add(messageBorder);
            _graphCanvas.Children.Add(shaBorder);
            _graphCanvas.Children.Add(authorDateLabel);

            var node = new BranchGraphNode
            {
                Commit = commit,
                Circle = circle,
                MessageLabel = messageLabel,
                ShaLabel = shaLabel,
                X = x,
                Y = y
            };

            circle.PointerPressed += (s, e) => OnCommitSelected(commit);
            messageBorder.PointerPressed += (s, e) => OnCommitSelected(commit);

            return node;
        }

        private async void ShowBranchSwitchDialog(string branchName)
        {
            var dialog = new ContentDialog
            {
                Title = "��������",
                Content = $"�T�w�n��������� '{branchName}' �ܡH\n\n�o�N���ܱz���u�@�ؿ����e�C",
                PrimaryButtonText = "����",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _gitService.CheckoutBranch(branchName);
                    LoadBranchData();
                    ShowMessage($"�w���������: {branchName}");
                }
                catch (Exception ex)
                {
                    ShowMessage($"�������䥢��: {ex.Message}");
                }
            }
        }

        private void OnCommitSelected(CommitInfo commit)
        {
            ShowCommitDetails(commit);
        }

        private void ShowCommitDetails(CommitInfo commit)
        {
            _commitDetailsPanel.Children.Clear();

            var shaPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var shaLabel = new TextBlock
            {
                Text = "����SHA:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var shaValue = new TextBlock
            {
                Text = commit.ShortSha,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246))
            };
            shaPanel.Children.Add(shaLabel);
            shaPanel.Children.Add(shaValue);

            var messagePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var messageLabel = new TextBlock
            {
                Text = "����T��:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var messageValue = new TextBlock
            {
                Text = commit.Message,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            messagePanel.Children.Add(messageLabel);
            messagePanel.Children.Add(messageValue);

            var authorPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var authorLabel = new TextBlock
            {
                Text = "�@��:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var authorValue = new TextBlock
            {
                Text = commit.Author,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            authorPanel.Children.Add(authorLabel);
            authorPanel.Children.Add(authorValue);

            var datePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var dateLabel = new TextBlock
            {
                Text = "������:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var dateValue = new TextBlock
            {
                Text = commit.Date.ToString("yyyy/MM/dd HH:mm"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            datePanel.Children.Add(dateLabel);
            datePanel.Children.Add(dateValue);

            var actionPanel = new StackPanel { Margin = new Thickness(0, 20, 0, 0), Spacing = 10 };

            var viewChangesButton = new Button
            {
                Content = "�d���ܧ�",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 13,
                Padding = new Thickness(12, 8, 12, 8),
                Background = new SolidColorBrush(Color.FromArgb(255, 99, 102, 241)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(0)
            };
            viewChangesButton.Click += (s, e) => ViewCommitChanges(commit);

            var revertButton = new Button
            {
                Content = "�w���^�_",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 13,
                Padding = new Thickness(12, 8, 12, 8),
                Background = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(0)
            };
            revertButton.Click += (s, e) => RevertToCommit(commit);

            actionPanel.Children.Add(viewChangesButton);
            actionPanel.Children.Add(revertButton);

            _commitDetailsPanel.Children.Add(shaPanel);
            _commitDetailsPanel.Children.Add(messagePanel);
            _commitDetailsPanel.Children.Add(authorPanel);
            _commitDetailsPanel.Children.Add(datePanel);
            _commitDetailsPanel.Children.Add(actionPanel);
        }

        private void ViewCommitChanges(CommitInfo commit)
        {
            ShowMessage($"�d�ݴ��� {commit.ShortSha} ���ܧ�]�\��}�o���^");
        }

        private async void RevertToCommit(CommitInfo commit)
        {
            var dialog = new ContentDialog
            {
                Title = "�T�{�^�_����",
                Content = $"�T�w�n�^�_�촣�� {commit.ShortSha} �ܡH\n\n�o�N�Ыؤ@�ӷs������ӺM�P���᪺�Ҧ��ܧ�C",
                PrimaryButtonText = "�T�{�^�_",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    var userSettings = ((App)App.Current).GetCurrentUserSettings();
                    if (userSettings != null)
                    {
                        _gitService.RevertToCommit(commit.Sha, userSettings.AuthorName, userSettings.AuthorEmail);
                        LoadBranchData();
                        ShowMessage($"�w�^�_�쪩��: {commit.ShortSha}");
                    }
                    else
                    {
                        ShowMessage("�L�k����Τ�]�w");
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"�^�_��������: {ex.Message}");
                }
            }
        }

        private async void CreateBranchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "�إ߷s����",
                PrimaryButtonText = "�إ�",
                SecondaryButtonText = "����",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };

            var panel = new StackPanel { Spacing = 12 };

            var label = new TextBlock
            {
                Text = "����W��:",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };

            var textBox = new TextBox
            {
                PlaceholderText = "�п�J����W�� (�Ҧp: feature/new-feature)",
                Margin = new Thickness(0, 4, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99))
            };

            var infoText = new TextBlock
            {
                Text = "��ĳ������R�W�W�d�G\n? feature/�\��W��\n? bugfix/�״_�W��\n? hotfix/���״_\n? release/������",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)),
                Margin = new Thickness(0, 8, 0, 0)
            };

            panel.Children.Add(label);
            panel.Children.Add(textBox);
            panel.Children.Add(infoText);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                try
                {
                    _gitService.CreateBranch(textBox.Text.Trim());
                    LoadBranchData();
                    ShowMessage($"�w�إߤ���: {textBox.Text.Trim()}");
                }
                catch (Exception ex)
                {
                    ShowMessage($"�إߤ��䥢��: {ex.Message}");
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBranchData();
            ShowMessage("�w���s��z");
        }

        private void FitToWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ZoomToFactor(1.0f);
                _scrollViewer.ScrollToHorizontalOffset(0);
                _scrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "�T��",
                Content = message,
                CloseButtonText = "�T�w",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark
            };
            await dialog.ShowAsync();
        }
    }

    public class BranchGraphNode
    {
        public CommitInfo Commit { get; set; } = null!;
        public Ellipse Circle { get; set; } = null!;
        public TextBlock MessageLabel { get; set; } = null!;
        public TextBlock ShaLabel { get; set; } = null!;
        public double X { get; set; }
        public double Y { get; set; }
    }
}