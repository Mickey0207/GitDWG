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

        public BranchGraphWindow(GitService gitService)
        {
            _gitService = gitService;
            _graphNodes = new List<BranchGraphNode>();
            _commits = new List<CommitInfo>();
            _branches = new List<string>();

            this.Title = "GitDWG - ����ϧκ޲z";
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            
            CreateUI();
            LoadBranchData();
            
            this.Activate();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 17, 24, 39)) // �`��D�I��
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // ����C��
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // �ϧΰϰ�
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) }); // �ԲӸ�T

            // ��������C��
            CreateBranchListPanel(mainGrid);
            
            // �����ϧΰϰ�
            CreateGraphArea(mainGrid);
            
            // �k���ԲӸ�T
            CreateDetailsPanel(mainGrid);

            this.Content = mainGrid;
        }

        private void CreateBranchListPanel(Grid mainGrid)
        {
            var leftBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // �`�⭱�O�I��
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)), // �`�����
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(16)
            };

            var leftPanel = new StackPanel();
            
            // ���D
            var branchTitle = new TextBlock
            {
                Text = "����޲z",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)) // �L���r
            };
            leftPanel.Children.Add(branchTitle);

            // �s�W������s
            var createBranchButton = new Button
            {
                Content = "�إ߷s����",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 16),
                Background = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(0)
            };
            createBranchButton.Click += CreateBranchButton_Click;
            leftPanel.Children.Add(createBranchButton);

            // ����C����D
            var branchListTitle = new TextBlock
            {
                Text = "�Ҧ�����",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)) // ���Ǧ��r
            };
            leftPanel.Children.Add(branchListTitle);

            // ����C��
            _branchListPanel = new StackPanel();
            leftPanel.Children.Add(_branchListPanel);

            leftBorder.Child = leftPanel;
            Grid.SetColumn(leftBorder, 0);
            mainGrid.Children.Add(leftBorder);
        }

        private void CreateGraphArea(Grid mainGrid)
        {
            var centerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 30, 42)), // �`��ϧΰϭI��
                Padding = new Thickness(16)
            };

            var centerPanel = new StackPanel();
            
            // ���D
            var graphTitle = new TextBlock
            {
                Text = "����ϧ�",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)) // �L���r
            };
            centerPanel.Children.Add(graphTitle);

            // �u��C
            var toolBar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var refreshButton = new Button
            {
                Content = "���s��z",
                FontSize = 12,
                Padding = new Thickness(12, 6, 12, 6),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)),
                BorderThickness = new Thickness(0)
            };
            refreshButton.Click += RefreshButton_Click;

            var fitToWindowButton = new Button
            {
                Content = "�A�X����",
                FontSize = 12,
                Padding = new Thickness(12, 6, 12, 6),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)),
                BorderThickness = new Thickness(0)
            };
            fitToWindowButton.Click += FitToWindowButton_Click;

            toolBar.Children.Add(refreshButton);
            toolBar.Children.Add(fitToWindowButton);
            centerPanel.Children.Add(toolBar);

            // �ϧεe��
            _graphCanvas = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // �`��e���I��
                MinHeight = 600
            };

            _scrollViewer = new ScrollViewer
            {
                Content = _graphCanvas,
                ZoomMode = ZoomMode.Enabled,
                HorizontalScrollMode = ScrollMode.Enabled,
                VerticalScrollMode = ScrollMode.Enabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 30, 42))
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
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // �`�⭱�O�I��
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)), // �`�����
                BorderThickness = new Thickness(1, 0, 0, 0),
                Padding = new Thickness(16)
            };

            var rightPanel = new StackPanel();
            
            var detailsTitle = new TextBlock
            {
                Text = "����ԲӸ�T",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)) // �L���r
            };
            rightPanel.Children.Add(detailsTitle);

            _commitDetailsPanel = new StackPanel();
            
            var noSelectionText = new TextBlock
            {
                Text = "�I������`�I�H�d�ݸԲӸ�T",
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)), // ���Ǧ��r
                TextWrapping = TextWrapping.Wrap
            };
            _commitDetailsPanel.Children.Add(noSelectionText);

            rightPanel.Children.Add(_commitDetailsPanel);
            rightBorder.Child = rightPanel;
            Grid.SetColumn(rightBorder, 2);
            mainGrid.Children.Add(rightBorder);
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

                // ���J����C��
                _branches = _gitService.GetBranches().ToList();
                UpdateBranchList();

                // ���J������v
                _commits = _gitService.GetCommitHistory(50).ToList();
                CreateBranchGraph();
            }
            catch (Exception ex)
            {
                ShowMessage($"���J�����ƥ���: {ex.Message}");
            }
        }

        private void UpdateBranchList()
        {
            _branchListPanel.Children.Clear();
            var currentBranch = _gitService.GetCurrentBranch();

            foreach (var branch in _branches)
            {
                var branchItem = new Border
                {
                    Background = branch == currentBranch ? 
                        new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)) : // �ŦⰪ�G��e����
                        new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), // �z���I��
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)), // �`�����
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 2, 0, 2)
                };

                var branchPanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                // ����ϥ�
                var branchIcon = new TextBlock
                {
                    Text = branch == currentBranch ? "��" : "��",
                    FontSize = 12,
                    Foreground = branch == currentBranch ?
                        new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) : // �զ�ϥ�
                        new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)), // �Ǧ�ϥ�
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var branchName = new TextBlock
                {
                    Text = branch,
                    FontSize = 13,
                    FontWeight = branch == currentBranch ? 
                        Microsoft.UI.Text.FontWeights.SemiBold : 
                        Microsoft.UI.Text.FontWeights.Normal,
                    Foreground = branch == currentBranch ?
                        new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) : // �զ��r
                        new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)), // �L�Ǥ�r
                    VerticalAlignment = VerticalAlignment.Center
                };

                branchPanel.Children.Add(branchIcon);
                branchPanel.Children.Add(branchName);
                branchItem.Child = branchPanel;

                // �K�[�I���ƥ�
                branchItem.PointerPressed += (s, e) => OnBranchSelected(branch);
                
                _branchListPanel.Children.Add(branchItem);
            }
        }

        private void CreateBranchGraph()
        {
            _graphCanvas.Children.Clear();
            _graphNodes.Clear();

            if (!_commits.Any()) return;

            // ���R���䵲�c
            var branchCommits = AnalyzeBranchStructure();
            
            const double nodeSpacing = 60;
            const double branchSpacing = 80;
            const double nodeRadius = 8;
            const double startX = 50;
            const double startY = 30;

            // ø�s����u�M�`�I
            int commitIndex = 0;
            foreach (var commit in _commits)
            {
                // �T�w�Ӵ����ݩ���Ӥ���
                var branchIndex = GetBranchIndex(commit, branchCommits);
                
                var x = startX + branchIndex * branchSpacing;
                var y = startY + commitIndex * nodeSpacing;

                // �Ыش���`�I
                var node = CreateCommitNode(commit, x, y, nodeRadius, branchIndex);
                _graphNodes.Add(node);

                // ø�s�s���u�������
                if (commitIndex > 0)
                {
                    DrawConnectionLines(commit, x, y, nodeRadius, branchCommits, startX, startY, nodeSpacing, branchSpacing);
                }

                commitIndex++;
            }

            // ø�s�������
            DrawBranchLabels(branchCommits, startX, startY, branchSpacing);

            // �վ�e���j�p
            AdjustCanvasSize(startX, startY, nodeSpacing, branchSpacing, branchCommits.Count);
        }

        private Dictionary<string, List<CommitInfo>> AnalyzeBranchStructure()
        {
            var branchCommits = new Dictionary<string, List<CommitInfo>>();
            
            // ���C�Ӥ���Ыش���C��
            foreach (var branch in _branches)
            {
                branchCommits[branch] = new List<CommitInfo>();
            }

            // ²�ƪ����G�N������t������
            // ������Τ��ݭn�������Git���R
            var mainBranch = _branches.Contains("main") ? "main" : 
                            _branches.Contains("master") ? "master" : 
                            _branches.FirstOrDefault() ?? "main";

            // �N�Ҧ���������t���D����
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
            // ²�ƪ����G�ھڴ���T���Χ@�̨ӧP�_����
            // ������Τ��ݭn���T��Git������R
            
            if (commit.Message.ToLower().Contains("feature") || commit.Message.ToLower().Contains("feat"))
                return 1; // feature����
            else if (commit.Message.ToLower().Contains("fix") || commit.Message.ToLower().Contains("bug"))
                return 2; // bugfix����
            else if (commit.Message.ToLower().Contains("hotfix"))
                return 3; // hotfix����
            
            return 0; // �D����
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
                // �P�@���䪺���u�s��
                var straightLine = new Line
                {
                    X1 = x,
                    Y1 = prevY + nodeRadius,
                    X2 = x,
                    Y2 = y - nodeRadius,
                    Stroke = GetBranchColor(currentBranchIndex),
                    StrokeThickness = 3
                };
                Canvas.SetZIndex(straightLine, 0);
                _graphCanvas.Children.Add(straightLine);
            }
            else
            {
                // ���P���䪺�s���s���]�X�֩Τ��e�^
                var path = new Microsoft.UI.Xaml.Shapes.Path
                {
                    Stroke = GetBranchColor(currentBranchIndex),
                    StrokeThickness = 2,
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
                Windows.UI.Color.FromArgb(255, 59, 130, 246),   // �Ŧ� - main
                Windows.UI.Color.FromArgb(255, 16, 185, 129),   // ��� - feature
                Windows.UI.Color.FromArgb(255, 245, 158, 11),   // ���� - bugfix
                Windows.UI.Color.FromArgb(255, 239, 68, 68),    // ���� - hotfix
                Windows.UI.Color.FromArgb(255, 168, 85, 247),   // ���� - ��L
                Windows.UI.Color.FromArgb(255, 236, 72, 153),   // ���� - ��L
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

            // �Ыب��뺸���u
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
            var branchNames = new[] { "main", "feature", "bugfix", "hotfix" };
            
            for (int i = 0; i < Math.Min(branchNames.Length, 4); i++)
            {
                var branchLabel = new Border
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(180, 51, 65, 85)),
                    Padding = new Thickness(8, 4, 8, 4),
                    CornerRadius = new CornerRadius(12),
                    BorderBrush = GetBranchColor(i),
                    BorderThickness = new Thickness(2)
                };

                var labelText = new TextBlock
                {
                    Text = branchNames[i],
                    FontSize = 11,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = GetBranchColor(i)
                };

                branchLabel.Child = labelText;
                Canvas.SetLeft(branchLabel, startX + i * branchSpacing - 20);
                Canvas.SetTop(branchLabel, startY - 25);
                Canvas.SetZIndex(branchLabel, 3);
                
                _graphCanvas.Children.Add(branchLabel);
            }
        }

        private void AdjustCanvasSize(double startX, double startY, double nodeSpacing, 
            double branchSpacing, int branchCount)
        {
            if (_commits.Any())
            {
                var maxY = startY + (_commits.Count - 1) * nodeSpacing + 50;
                var maxX = startX + branchCount * branchSpacing + 100;
                
                _graphCanvas.Height = Math.Max(600, maxY);
                _graphCanvas.Width = Math.Max(600, maxX);
            }
        }

        private BranchGraphNode CreateCommitNode(CommitInfo commit, double x, double y, double radius, int branchIndex)
        {
            // �`�I��� - �ϥΤ����C��
            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = GetBranchColor(branchIndex),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // �`�����
                StrokeThickness = 2
            };
            Canvas.SetLeft(circle, x - radius);
            Canvas.SetTop(circle, y - radius);
            Canvas.SetZIndex(circle, 2);

            // ����T���I��
            var messageBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 51, 65, 85)), // �b�z���`��I��
                Padding = new Thickness(6, 3, 6, 3),
                MaxWidth = 220,
                CornerRadius = new CornerRadius(4),
                BorderBrush = GetBranchColor(branchIndex),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(messageBorder, x + radius + 12);
            Canvas.SetTop(messageBorder, y - 10);
            Canvas.SetZIndex(messageBorder, 1);

            // ����T������
            var messageLabel = new TextBlock
            {
                Text = commit.Message.Length > 45 ? 
                    commit.Message.Substring(0, 45) + "..." : 
                    commit.Message,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)) // �L���r
            };
            messageBorder.Child = messageLabel;

            // SHA�I��
            var shaBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(160, 75, 85, 99)), // �b�z���`�ǭI��
                Padding = new Thickness(4, 2, 4, 2),
                CornerRadius = new CornerRadius(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(shaBorder, x + radius + 12);
            Canvas.SetTop(shaBorder, y + 6);
            Canvas.SetZIndex(shaBorder, 1);

            // SHA����
            var shaLabel = new TextBlock
            {
                Text = commit.ShortSha,
                FontSize = 9,
                FontFamily = new FontFamily("Consolas"),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 156, 163, 175)) // ���Ǥ�r
            };
            shaBorder.Child = shaLabel;

            // �@�̩M�������
            var authorDateLabel = new TextBlock
            {
                Text = $"{commit.Author} ? {commit.Date:MM/dd HH:mm}",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)),
                MaxWidth = 200
            };
            Canvas.SetLeft(authorDateLabel, x + radius + 12);
            Canvas.SetTop(authorDateLabel, y + 22);
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

            // �K�[�I���ƥ�
            circle.PointerPressed += (s, e) => OnCommitSelected(commit);
            messageBorder.PointerPressed += (s, e) => OnCommitSelected(commit);

            return node;
        }

        private void OnBranchSelected(string branchName)
        {
            try
            {
                if (branchName != _gitService.GetCurrentBranch())
                {
                    ShowBranchSwitchDialog(branchName);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"�������䥢��: {ex.Message}");
            }
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
                RequestedTheme = ElementTheme.Dark // �`���ܮ�
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _gitService.CheckoutBranch(branchName);
                    LoadBranchData(); // ���s���J���
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

            // ����SHA
            var shaPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var shaLabel = new TextBlock
            {
                Text = "����SHA:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 4),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var shaValue = new TextBlock
            {
                Text = commit.ShortSha,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246))
            };
            shaPanel.Children.Add(shaLabel);
            shaPanel.Children.Add(shaValue);

            // ����T��
            var messagePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var messageLabel = new TextBlock
            {
                Text = "����T��:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 4),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var messageValue = new TextBlock
            {
                Text = commit.Message,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            messagePanel.Children.Add(messageLabel);
            messagePanel.Children.Add(messageValue);

            // �@��
            var authorPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var authorLabel = new TextBlock
            {
                Text = "�@��:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 4),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var authorValue = new TextBlock
            {
                Text = commit.Author,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            authorPanel.Children.Add(authorLabel);
            authorPanel.Children.Add(authorValue);

            // ���
            var datePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var dateLabel = new TextBlock
            {
                Text = "������:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 4),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            var dateValue = new TextBlock
            {
                Text = commit.Date.ToString("yyyy/MM/dd HH:mm"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184))
            };
            datePanel.Children.Add(dateLabel);
            datePanel.Children.Add(dateValue);

            // �ާ@���s
            var actionPanel = new StackPanel { Margin = new Thickness(0, 16, 0, 0), Spacing = 8 };
            
            var viewChangesButton = new Button
            {
                Content = "�d���ܧ�",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 12,
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush(Color.FromArgb(255, 99, 102, 241)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(0)
            };
            viewChangesButton.Click += (s, e) => ViewCommitChanges(commit);

            var revertButton = new Button
            {
                Content = "�w���^�_",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 12,
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(4),
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
                RequestedTheme = ElementTheme.Dark // �`���ܮ�
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // �o�̻ݭn�q�D��������Τ�]�w
                    var userSettings = ((App)App.Current).GetCurrentUserSettings();
                    if (userSettings != null)
                    {
                        _gitService.RevertToCommit(commit.Sha, userSettings.AuthorName, userSettings.AuthorEmail);
                        LoadBranchData(); // ���s���J���
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
                RequestedTheme = ElementTheme.Dark // �`���ܮ�
            };

            var textBox = new TextBox
            {
                PlaceholderText = "�п�J����W��",
                Margin = new Thickness(0, 8, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99))
            };

            var panel = new StackPanel();
            var label = new TextBlock 
            { 
                Text = "����W��:",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240))
            };
            panel.Children.Add(label);
            panel.Children.Add(textBox);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                try
                {
                    _gitService.CreateBranch(textBox.Text.Trim());
                    LoadBranchData(); // ���s���J���
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
                RequestedTheme = ElementTheme.Dark // �`���ܮ�
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