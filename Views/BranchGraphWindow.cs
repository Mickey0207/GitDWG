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

            this.Title = "GitDWG - 分支圖形管理";
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            
            CreateUI();
            LoadBranchData();
            
            this.Activate();
        }

        private void CreateUI()
        {
            var mainGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 17, 24, 39)) // 深色主背景
            };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // 分支列表
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 圖形區域
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) }); // 詳細資訊

            // 左側分支列表
            CreateBranchListPanel(mainGrid);
            
            // 中間圖形區域
            CreateGraphArea(mainGrid);
            
            // 右側詳細資訊
            CreateDetailsPanel(mainGrid);

            this.Content = mainGrid;
        }

        private void CreateBranchListPanel(Grid mainGrid)
        {
            var leftBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // 深色面板背景
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(16)
            };

            var leftPanel = new StackPanel();
            
            // 標題
            var branchTitle = new TextBlock
            {
                Text = "分支管理",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)) // 淺色文字
            };
            leftPanel.Children.Add(branchTitle);

            // 新增分支按鈕
            var createBranchButton = new Button
            {
                Content = "建立新分支",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 16),
                Background = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(0)
            };
            createBranchButton.Click += CreateBranchButton_Click;
            leftPanel.Children.Add(createBranchButton);

            // 分支列表標題
            var branchListTitle = new TextBlock
            {
                Text = "所有分支",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)) // 中灰色文字
            };
            leftPanel.Children.Add(branchListTitle);

            // 分支列表
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
                Background = new SolidColorBrush(Color.FromArgb(255, 24, 30, 42)), // 深色圖形區背景
                Padding = new Thickness(16)
            };

            var centerPanel = new StackPanel();
            
            // 標題
            var graphTitle = new TextBlock
            {
                Text = "提交圖形",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)) // 淺色文字
            };
            centerPanel.Children.Add(graphTitle);

            // 工具列
            var toolBar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var refreshButton = new Button
            {
                Content = "重新整理",
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
                Content = "適合視窗",
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

            // 圖形畫布
            _graphCanvas = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // 深色畫布背景
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
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // 深色面板背景
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                BorderThickness = new Thickness(1, 0, 0, 0),
                Padding = new Thickness(16)
            };

            var rightPanel = new StackPanel();
            
            var detailsTitle = new TextBlock
            {
                Text = "提交詳細資訊",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)) // 淺色文字
            };
            rightPanel.Children.Add(detailsTitle);

            _commitDetailsPanel = new StackPanel();
            
            var noSelectionText = new TextBlock
            {
                Text = "點擊提交節點以查看詳細資訊",
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)), // 中灰色文字
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
                    ShowMessage("Git儲存庫未初始化");
                    return;
                }

                // 載入分支列表
                _branches = _gitService.GetBranches().ToList();
                UpdateBranchList();

                // 載入提交歷史
                _commits = _gitService.GetCommitHistory(50).ToList();
                CreateBranchGraph();
            }
            catch (Exception ex)
            {
                ShowMessage($"載入分支資料失敗: {ex.Message}");
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
                        new SolidColorBrush(Color.FromArgb(255, 59, 130, 246)) : // 藍色高亮當前分支
                        new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), // 透明背景
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)), // 深灰邊框
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 2, 0, 2)
                };

                var branchPanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                // 分支圖示
                var branchIcon = new TextBlock
                {
                    Text = branch == currentBranch ? "●" : "○",
                    FontSize = 12,
                    Foreground = branch == currentBranch ?
                        new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) : // 白色圖示
                        new SolidColorBrush(Color.FromArgb(255, 148, 163, 184)), // 灰色圖示
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
                        new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) : // 白色文字
                        new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)), // 淺灰文字
                    VerticalAlignment = VerticalAlignment.Center
                };

                branchPanel.Children.Add(branchIcon);
                branchPanel.Children.Add(branchName);
                branchItem.Child = branchPanel;

                // 添加點擊事件
                branchItem.PointerPressed += (s, e) => OnBranchSelected(branch);
                
                _branchListPanel.Children.Add(branchItem);
            }
        }

        private void CreateBranchGraph()
        {
            _graphCanvas.Children.Clear();
            _graphNodes.Clear();

            if (!_commits.Any()) return;

            // 分析分支結構
            var branchCommits = AnalyzeBranchStructure();
            
            const double nodeSpacing = 60;
            const double branchSpacing = 80;
            const double nodeRadius = 8;
            const double startX = 50;
            const double startY = 30;

            // 繪製分支線和節點
            int commitIndex = 0;
            foreach (var commit in _commits)
            {
                // 確定該提交屬於哪個分支
                var branchIndex = GetBranchIndex(commit, branchCommits);
                
                var x = startX + branchIndex * branchSpacing;
                var y = startY + commitIndex * nodeSpacing;

                // 創建提交節點
                var node = CreateCommitNode(commit, x, y, nodeRadius, branchIndex);
                _graphNodes.Add(node);

                // 繪製連接線到父提交
                if (commitIndex > 0)
                {
                    DrawConnectionLines(commit, x, y, nodeRadius, branchCommits, startX, startY, nodeSpacing, branchSpacing);
                }

                commitIndex++;
            }

            // 繪製分支標籤
            DrawBranchLabels(branchCommits, startX, startY, branchSpacing);

            // 調整畫布大小
            AdjustCanvasSize(startX, startY, nodeSpacing, branchSpacing, branchCommits.Count);
        }

        private Dictionary<string, List<CommitInfo>> AnalyzeBranchStructure()
        {
            var branchCommits = new Dictionary<string, List<CommitInfo>>();
            
            // 為每個分支創建提交列表
            foreach (var branch in _branches)
            {
                branchCommits[branch] = new List<CommitInfo>();
            }

            // 簡化版本：將提交分配給分支
            // 實際應用中需要更複雜的Git分析
            var mainBranch = _branches.Contains("main") ? "main" : 
                            _branches.Contains("master") ? "master" : 
                            _branches.FirstOrDefault() ?? "main";

            // 將所有提交先分配給主分支
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
            // 簡化版本：根據提交訊息或作者來判斷分支
            // 實際應用中需要更精確的Git分支分析
            
            if (commit.Message.ToLower().Contains("feature") || commit.Message.ToLower().Contains("feat"))
                return 1; // feature分支
            else if (commit.Message.ToLower().Contains("fix") || commit.Message.ToLower().Contains("bug"))
                return 2; // bugfix分支
            else if (commit.Message.ToLower().Contains("hotfix"))
                return 3; // hotfix分支
            
            return 0; // 主分支
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
                // 同一分支的直線連接
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
                // 不同分支的彎曲連接（合併或分叉）
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
                Windows.UI.Color.FromArgb(255, 59, 130, 246),   // 藍色 - main
                Windows.UI.Color.FromArgb(255, 16, 185, 129),   // 綠色 - feature
                Windows.UI.Color.FromArgb(255, 245, 158, 11),   // 黃色 - bugfix
                Windows.UI.Color.FromArgb(255, 239, 68, 68),    // 紅色 - hotfix
                Windows.UI.Color.FromArgb(255, 168, 85, 247),   // 紫色 - 其他
                Windows.UI.Color.FromArgb(255, 236, 72, 153),   // 粉色 - 其他
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

            // 創建貝塞爾曲線
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
            // 節點圓圈 - 使用分支顏色
            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = GetBranchColor(branchIndex),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)), // 深色邊框
                StrokeThickness = 2
            };
            Canvas.SetLeft(circle, x - radius);
            Canvas.SetTop(circle, y - radius);
            Canvas.SetZIndex(circle, 2);

            // 提交訊息背景
            var messageBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 51, 65, 85)), // 半透明深色背景
                Padding = new Thickness(6, 3, 6, 3),
                MaxWidth = 220,
                CornerRadius = new CornerRadius(4),
                BorderBrush = GetBranchColor(branchIndex),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(messageBorder, x + radius + 12);
            Canvas.SetTop(messageBorder, y - 10);
            Canvas.SetZIndex(messageBorder, 1);

            // 提交訊息標籤
            var messageLabel = new TextBlock
            {
                Text = commit.Message.Length > 45 ? 
                    commit.Message.Substring(0, 45) + "..." : 
                    commit.Message,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 226, 232, 240)) // 淺色文字
            };
            messageBorder.Child = messageLabel;

            // SHA背景
            var shaBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(160, 75, 85, 99)), // 半透明深灰背景
                Padding = new Thickness(4, 2, 4, 2),
                CornerRadius = new CornerRadius(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)),
                BorderThickness = new Thickness(1)
            };
            Canvas.SetLeft(shaBorder, x + radius + 12);
            Canvas.SetTop(shaBorder, y + 6);
            Canvas.SetZIndex(shaBorder, 1);

            // SHA標籤
            var shaLabel = new TextBlock
            {
                Text = commit.ShortSha,
                FontSize = 9,
                FontFamily = new FontFamily("Consolas"),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 156, 163, 175)) // 中灰文字
            };
            shaBorder.Child = shaLabel;

            // 作者和日期標籤
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

            // 添加點擊事件
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
                ShowMessage($"切換分支失敗: {ex.Message}");
            }
        }

        private async void ShowBranchSwitchDialog(string branchName)
        {
            var dialog = new ContentDialog
            {
                Title = "切換分支",
                Content = $"確定要切換到分支 '{branchName}' 嗎？\n\n這將改變您的工作目錄內容。",
                PrimaryButtonText = "切換",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark // 深色對話框
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _gitService.CheckoutBranch(branchName);
                    LoadBranchData(); // 重新載入資料
                    ShowMessage($"已切換到分支: {branchName}");
                }
                catch (Exception ex)
                {
                    ShowMessage($"切換分支失敗: {ex.Message}");
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

            // 提交SHA
            var shaPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var shaLabel = new TextBlock
            {
                Text = "提交SHA:",
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

            // 提交訊息
            var messagePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var messageLabel = new TextBlock
            {
                Text = "提交訊息:",
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

            // 作者
            var authorPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var authorLabel = new TextBlock
            {
                Text = "作者:",
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

            // 日期
            var datePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            var dateLabel = new TextBlock
            {
                Text = "提交日期:",
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

            // 操作按鈕
            var actionPanel = new StackPanel { Margin = new Thickness(0, 16, 0, 0), Spacing = 8 };
            
            var viewChangesButton = new Button
            {
                Content = "查看變更",
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
                Content = "安全回復",
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
            ShowMessage($"查看提交 {commit.ShortSha} 的變更（功能開發中）");
        }

        private async void RevertToCommit(CommitInfo commit)
        {
            var dialog = new ContentDialog
            {
                Title = "確認回復版本",
                Content = $"確定要回復到提交 {commit.ShortSha} 嗎？\n\n這將創建一個新的提交來撤銷之後的所有變更。",
                PrimaryButtonText = "確認回復",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark // 深色對話框
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // 這裡需要從主應用獲取用戶設定
                    var userSettings = ((App)App.Current).GetCurrentUserSettings();
                    if (userSettings != null)
                    {
                        _gitService.RevertToCommit(commit.Sha, userSettings.AuthorName, userSettings.AuthorEmail);
                        LoadBranchData(); // 重新載入資料
                        ShowMessage($"已回復到版本: {commit.ShortSha}");
                    }
                    else
                    {
                        ShowMessage("無法獲取用戶設定");
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"回復版本失敗: {ex.Message}");
                }
            }
        }

        private async void CreateBranchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "建立新分支",
                PrimaryButtonText = "建立",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark // 深色對話框
            };

            var textBox = new TextBox
            {
                PlaceholderText = "請輸入分支名稱",
                Margin = new Thickness(0, 8, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99))
            };

            var panel = new StackPanel();
            var label = new TextBlock 
            { 
                Text = "分支名稱:",
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
                    LoadBranchData(); // 重新載入資料
                    ShowMessage($"已建立分支: {textBox.Text.Trim()}");
                }
                catch (Exception ex)
                {
                    ShowMessage($"建立分支失敗: {ex.Message}");
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBranchData();
            ShowMessage("已重新整理");
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
                Title = "訊息",
                Content = message,
                CloseButtonText = "確定",
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = ElementTheme.Dark // 深色對話框
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