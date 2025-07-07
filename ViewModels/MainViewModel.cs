using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GitDWG.Services;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GitDWG.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GitService _gitService;
        private readonly AutoCadCompareService _autocadService;
        private readonly UserSettingsService _userSettingsService;
        
        private string _repositoryPath = string.Empty;
        private string _currentBranch = string.Empty;
        private string _statusMessage = string.Empty;
        private string _commitMessage = string.Empty;
        private string _authorName = string.Empty;
        private string _authorEmail = string.Empty;
        private string _commitButtonTooltip = string.Empty;
        private bool _isRepositoryLoaded;
        private CommitInfo? _selectedCommit;

        public MainViewModel(UserSettings userSettings)
        {
            _gitService = new GitService(string.Empty);
            _autocadService = new AutoCadCompareService();
            _userSettingsService = new UserSettingsService();
            
            Commits = new ObservableCollection<CommitInfo>();
            Branches = new ObservableCollection<string>();
            StatusItems = new ObservableCollection<string>();
            ChangedFiles = new ObservableCollection<string>();

            // 使用傳入的用戶設定
            AuthorName = userSettings.AuthorName;
            AuthorEmail = userSettings.AuthorEmail;
            
            // 如果有上次的儲存庫路徑，自動載入
            if (!string.IsNullOrEmpty(userSettings.LastRepositoryPath) && 
                Directory.Exists(userSettings.LastRepositoryPath))
            {
                RepositoryPath = userSettings.LastRepositoryPath;
            }

            // 如果有AutoCAD路徑設定，自動套用
            if (!string.IsNullOrEmpty(userSettings.AutoCADPath) && 
                File.Exists(userSettings.AutoCADPath))
            {
                _autocadService.SetAutoCADPath(userSettings.AutoCADPath);
            }

            // 初始化命令
            InitializeCommands();
            
            // 更新狀態
            UpdateStatus();
            
            // 初始化提交按鈕提示
            UpdateCommitButtonTooltip();
        }

        // 保留原有的無參數構造函數以防向後兼容性問題
        public MainViewModel() : this(new UserSettings 
        { 
            AuthorName = Environment.UserName, 
            AuthorEmail = $"{Environment.UserName}@example.com" 
        })
        {
        }

        private void InitializeCommands()
        {
            SelectRepositoryCommand = new RelayCommand(async () => await SelectRepositoryAsync());
            InitializeRepositoryCommand = new RelayCommand(InitializeRepository);
            RefreshCommand = new RelayCommand(RefreshData);
            StageAllCommand = new RelayCommand(StageAllChanges);
            CommitCommand = new RelayCommand(CommitChanges, CanCommit);
            CreateBranchCommand = new RelayCommand(async () => await CreateBranchAsync());
            CheckoutBranchCommand = new RelayCommand<string>(CheckoutBranch);
            CompareCommitsCommand = new RelayCommand(async () => await CompareCommitsAsync(), CanCompareCommits);
            OpenDrawingCommand = new RelayCommand<string>(async (filePath) => await OpenDrawingAsync(filePath ?? string.Empty));
            SetAutoCADPathCommand = new RelayCommand(async () => await SetAutoCADPathAsync());
            
            // 新增的DWG相關命令
            ForceRefreshCommand = new RelayCommand(ForceRefresh);
            CheckCadFilesCommand = new RelayCommand(CheckCadFileStatus);
            DiagnoseCommitCommand = new RelayCommand(DiagnoseCommitIssues);
            EditUserSettingsCommand = new RelayCommand(async () => await EditUserSettingsAsync());
            
            // 新增版本回復命令
            RevertToCommitCommand = new RelayCommand(async () => await RevertToCommitAsync(), CanRevertToCommit);
            ResetToCommitCommand = new RelayCommand(async () => await ResetToCommitAsync(), CanResetToCommit);
        }

        #region Properties

        public string RepositoryPath
        {
            get => _repositoryPath;
            set
            {
                _repositoryPath = value ?? string.Empty;
                OnPropertyChanged();
                _gitService.SetRepository(_repositoryPath);
                
                // 保存到用戶設定
                if (!string.IsNullOrEmpty(_repositoryPath))
                {
                    var currentSettings = _userSettingsService.LoadSettings() ?? new UserSettings();
                    currentSettings.LastRepositoryPath = _repositoryPath;
                    _userSettingsService.SaveSettings(currentSettings);
                }
                
                RefreshData();
            }
        }

        public string CurrentBranch
        {
            get => _currentBranch;
            set
            {
                _currentBranch = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string CommitMessage
        {
            get => _commitMessage;
            set
            {
                var newValue = value ?? string.Empty;
                if (_commitMessage != newValue)  // 添加值比較
                {
                    _commitMessage = newValue;
                    OnPropertyChanged();
                    
                    try
                    {
                        UpdateCommitButtonTooltip();
                        ((RelayCommand)CommitCommand).RaiseCanExecuteChanged();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in CommitMessage setter: {ex.Message}");
                    }
                }
            }
        }

        public string AuthorName
        {
            get => _authorName;
            set
            {
                var newValue = value ?? string.Empty;
                if (_authorName != newValue)
                {
                    _authorName = newValue;
                    OnPropertyChanged();
                    
                    try
                    {
                        UpdateCommitButtonTooltip();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in AuthorName setter: {ex.Message}");
                    }
                }
            }
        }

        public string AuthorEmail
        {
            get => _authorEmail;
            set
            {
                var newValue = value ?? string.Empty;
                if (_authorEmail != newValue)
                {
                    _authorEmail = newValue;
                    OnPropertyChanged();
                    
                    try
                    {
                        UpdateCommitButtonTooltip();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in AuthorEmail setter: {ex.Message}");
                    }
                }
            }
        }

        public string CommitButtonTooltip
        {
            get => _commitButtonTooltip;
            set
            {
                _commitButtonTooltip = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public bool IsRepositoryLoaded
        {
            get => _isRepositoryLoaded;
            set
            {
                if (_isRepositoryLoaded != value)
                {
                    _isRepositoryLoaded = value;
                    OnPropertyChanged();
                    
                    try
                    {
                        UpdateCommitButtonTooltip();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in IsRepositoryLoaded setter: {ex.Message}");
                    }
                }
            }
        }

        public CommitInfo? SelectedCommit
        {
            get => _selectedCommit;
            set
            {
                if (_selectedCommit != value)  // 添加值比較避免不必要的更新
                {
                    _selectedCommit = value;
                    OnPropertyChanged();
                    
                    // 使用 try-catch 保護 RaiseCanExecuteChanged 調用
                    try
                    {
                        ((RelayCommand)CompareCommitsCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)RevertToCommitCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)ResetToCommitCommand).RaiseCanExecuteChanged();
                    }
                    catch (Exception ex)
                    {
                        // 記錄異常但不重新拋出，避免崩潰
                        System.Diagnostics.Debug.WriteLine($"Error in RaiseCanExecuteChanged: {ex.Message}");
                    }
                }
            }
        }

        public ObservableCollection<CommitInfo> Commits { get; }
        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> StatusItems { get; }
        public ObservableCollection<string> ChangedFiles { get; }

        #endregion

        #region Commands

        public ICommand SelectRepositoryCommand { get; private set; }
        public ICommand InitializeRepositoryCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand StageAllCommand { get; private set; }
        public ICommand CommitCommand { get; private set; }
        public ICommand CreateBranchCommand { get; private set; }
        public ICommand CheckoutBranchCommand { get; private set; }
        public ICommand CompareCommitsCommand { get; private set; }
        public ICommand OpenDrawingCommand { get; private set; }
        public ICommand SetAutoCADPathCommand { get; private set; }
        
        // 新增的DWG相關命令
        public ICommand ForceRefreshCommand { get; private set; }
        public ICommand CheckCadFilesCommand { get; private set; }
        public ICommand DiagnoseCommitCommand { get; private set; }
        public ICommand EditUserSettingsCommand { get; private set; }
        
        // 新增版本回復命令
        public ICommand RevertToCommitCommand { get; private set; }
        public ICommand ResetToCommitCommand { get; private set; }

        #endregion

        #region Command Implementations

        private async Task SelectRepositoryAsync()
        {
            try
            {
                var picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add("*");

                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                    var folder = await picker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        RepositoryPath = folder.Path;
                        StatusMessage = $"已選擇儲存庫: {folder.Path}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"選擇資料夾失敗: {ex.Message}";
            }
        }

        private void InitializeRepository()
        {
            if (string.IsNullOrEmpty(RepositoryPath))
            {
                StatusMessage = "請先選擇儲存庫資料夾";
                return;
            }

            try
            {
                _gitService.InitializeRepository();
                RefreshData();
                StatusMessage = "儲存庫初始化成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"初始化儲存庫失敗: {ex.Message}";
            }
        }

        private void RefreshData()
        {
            if (string.IsNullOrEmpty(RepositoryPath))
                return;

            try
            {
                // 強制重新整理儲存庫狀態
                _gitService.RefreshRepository();
                
                IsRepositoryLoaded = _gitService.IsRepositoryValid();
                
                if (IsRepositoryLoaded)
                {
                    CurrentBranch = _gitService.GetCurrentBranch();
                    
                    // 更新分支列表
                    Branches.Clear();
                    foreach (var branch in _gitService.GetBranches())
                    {
                        Branches.Add(branch);
                    }

                    // 更新提交歷史
                    Commits.Clear();
                    foreach (var commit in _gitService.GetCommitHistory())
                    {
                        Commits.Add(commit);
                    }

                    // 更新狀態 - 優先顯示CAD檔案狀態
                    StatusItems.Clear();
                    
                    // 先顯示CAD檔案的詳細狀態
                    var cadFileChanges = _gitService.GetCadFileChanges();
                    foreach (var cadFile in cadFileChanges)
                    {
                        var statusText = $"{cadFile.State}: {cadFile.FilePath}";
                        if (cadFile.IsLocked)
                        {
                            statusText += " (檔案使用中)";
                        }
                        statusText += $" ({FormatFileSize(cadFile.FileSize)})";
                        StatusItems.Add(statusText);
                    }
                    
                    // 再顯示其他檔案的狀態
                    foreach (var status in _gitService.GetStatusChanges())
                    {
                        // 避免重複顯示CAD檔案
                        if (!StatusItems.Any(s => s.Contains(status.Split(':')[1].Trim())))
                        {
                            StatusItems.Add(status);
                        }
                    }

                    // 檢查是否有.gitignore問題
                    if (_gitService.IsIgnoringCadFiles())
                    {
                        StatusMessage = $"儲存庫已載入 - 分支: {CurrentBranch} (注意: .gitignore可能忽略了CAD檔案)";
                    }
                    else
                    {
                        StatusMessage = $"儲存庫已載入 - 分支: {CurrentBranch}";
                    }
                }
                else
                {
                    StatusMessage = "所選資料夾不是有效的Git儲存庫";
                }

                // 更新提交按鈕提示
                UpdateCommitButtonTooltip();
            }
            catch (Exception ex)
            {
                StatusMessage = $"載入儲存庫失敗: {ex.Message}";
                UpdateCommitButtonTooltip();
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        private void StageAllChanges()
        {
            try
            {
                var result = _gitService.StageAllChangesWithResult();
                RefreshData();
                
                if (result.Success)
                {
                    StatusMessage = result.Message;
                    
                    // 如果有跳過的檔案，給出詳細資訊
                    if (result.SkippedCount > 0)
                    {
                        StatusMessage += $"\n跳過的檔案: {string.Join(", ", result.SkippedFiles)}";
                    }
                }
                else
                {
                    StatusMessage = result.Message;
                }

                // 更新提交按鈕提示
                UpdateCommitButtonTooltip();
            }
            catch (Exception ex)
            {
                StatusMessage = $"暫存變更失敗: {ex.Message}";
                UpdateCommitButtonTooltip();
            }
        }

        private void CommitChanges()
        {
            try
            {
                // 先檢查儲存庫狀態
                var statusInfo = _gitService.GetDetailedStatus();
                
                if (!statusInfo.CanCommit)
                {
                    StatusMessage = $"無法提交: {statusInfo.Message}";
                    return;
                }

                _gitService.Commit(CommitMessage, AuthorName, AuthorEmail);
                CommitMessage = string.Empty;
                RefreshData();
                StatusMessage = "提交成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"提交失敗: {ex.Message}";
            }
        }

        private bool CanCommit()
        {
            if (!IsRepositoryLoaded || 
                string.IsNullOrWhiteSpace(CommitMessage) || 
                string.IsNullOrWhiteSpace(AuthorName) || 
                string.IsNullOrWhiteSpace(AuthorEmail))
            {
                return false;
            }

            try
            {
                var statusInfo = _gitService.GetDetailedStatus();
                return statusInfo.CanCommit;
            }
            catch
            {
                return false;
            }
        }

        private bool CanCompareCommits()
        {
            try
            {
                // 直接使用私有字段避免觸發屬性訪問
                return _selectedCommit != null && _isRepositoryLoaded;
            }
            catch
            {
                // 如果發生任何異常，返回 false
                return false;
            }
        }

        private bool CanRevertToCommit()
        {
            try
            {
                return _selectedCommit != null && _isRepositoryLoaded;
            }
            catch
            {
                return false;
            }
        }

        private bool CanResetToCommit()
        {
            try
            {
                return _selectedCommit != null && _isRepositoryLoaded;
            }
            catch
            {
                return false;
            }
        }

        private async Task CreateBranchAsync()
        {
            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "建立新分支",
                        Content = new TextBox { PlaceholderText = "輸入分支名稱" },
                        PrimaryButtonText = "建立",
                        CloseButtonText = "取消",
                        XamlRoot = mainWindow.Content.XamlRoot
                    };

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        var textBox = dialog.Content as TextBox;
                        var branchName = textBox?.Text?.Trim();
                        
                        if (!string.IsNullOrEmpty(branchName))
                        {
                            _gitService.CreateBranch(branchName);
                            RefreshData();
                            StatusMessage = $"分支 '{branchName}' 已建立";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"建立分支失敗: {ex.Message}";
            }
        }

        private void CheckoutBranch(string? branchName)
        {
            if (string.IsNullOrEmpty(branchName)) return;

            try
            {
                _gitService.CheckoutBranch(branchName);
                RefreshData();
                StatusMessage = $"已切換至分支: {branchName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"切換分支失敗: {ex.Message}";
            }
        }

        private async Task CompareCommitsAsync()
        {
            if (SelectedCommit == null)
            {
                StatusMessage = "請選擇一個提交進行比較";
                return;
            }

            // 在開始時保存選中的提交資訊，避免在執行過程中丟失
            var targetCommit = SelectedCommit;

            // 檢查AutoCAD是否可用
            if (!_autocadService.IsAutoCADAvailable())
            {
                StatusMessage = "AutoCAD未找到，無法進行圖面比較。請點擊「設定AutoCAD路徑」按鈕設定AutoCAD的安裝路徑。";
                return;
            }

            try
            {
                StatusMessage = "正在分析提交變更...";
                
                // 簡化：與前一個提交比較
                var commits = Commits.ToList();
                var selectedIndex = commits.IndexOf(targetCommit);
                
                if (selectedIndex < commits.Count - 1)
                {
                    var previousCommit = commits[selectedIndex + 1];
                    var changedFiles = _gitService.GetChangedFilesBetweenCommits(previousCommit.Sha, targetCommit.Sha);
                    
                    ChangedFiles.Clear();
                    foreach (var file in changedFiles)
                    {
                        ChangedFiles.Add(file);
                    }

                    if (ChangedFiles.Count > 0)
                    {
                        StatusMessage = $"找到 {ChangedFiles.Count} 個變更的圖面檔案。正在準備AutoCAD比較...";
                        
                        // 顯示提示對話框
                        var mainWindow = ((App)App.Current).MainWindow;
                        if (mainWindow != null)
                        {
                            var dialog = new ContentDialog
                            {
                                Title = "圖面版本比較",
                                Content = $"即將使用AutoCAD開啟以下檔案進行比較：\n\n" +
                                         $"檔案：{ChangedFiles.First()}\n" +
                                         $"舊版本：{previousCommit.ShortSha} ({previousCommit.Message.Split('\n')[0]})\n" +
                                         $"新版本：{targetCommit.ShortSha} ({targetCommit.Message.Split('\n')[0]})\n\n" +
                                         $"AutoCAD將開啟兩個版本的檔案供您比較。",
                                PrimaryButtonText = "開始比較",
                                SecondaryButtonText = "取消",
                                DefaultButton = ContentDialogButton.Primary,
                                XamlRoot = mainWindow.Content.XamlRoot
                            };

                            var result = await dialog.ShowAsync();
                            
                            if (result == ContentDialogResult.Primary)
                            {
                                StatusMessage = "正在使用AutoCAD開啟圖面進行比較...";
                                await OpenDrawingForComparisonAsync(ChangedFiles.First(), previousCommit.Sha, targetCommit.Sha);
                            }
                            else
                            {
                                StatusMessage = "已取消圖面比較";
                            }
                        }
                    }
                    else
                    {
                        StatusMessage = "在選擇的提交之間沒有找到圖面檔案變更";
                    }
                }
                else
                {
                    StatusMessage = "選擇的提交沒有前一個提交可比較（這是第一個提交）";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"比較提交失敗: {ex.Message}";
            }
        }

        private async Task OpenDrawingForComparisonAsync(string filePath, string fromCommitSha, string toCommitSha)
        {
            try
            {
                // 使用Git checkout創建兩個版本的檔案
                var tempDir = Path.GetTempPath();
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var comparisonDir = Path.Combine(tempDir, $"GitDWG_Compare_{timestamp}");
                Directory.CreateDirectory(comparisonDir);

                var fileName = Path.GetFileName(filePath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                var fileExt = Path.GetExtension(filePath);

                var oldVersionPath = Path.Combine(comparisonDir, $"{fileNameWithoutExt}_舊版本_{fromCommitSha.Substring(0, 7)}{fileExt}");
                var newVersionPath = Path.Combine(comparisonDir, $"{fileNameWithoutExt}_新版本_{toCommitSha.Substring(0, 7)}{fileExt}");

                // 從Git提取兩個版本的檔案
                var oldVersionContent = _gitService.GetFileContentFromCommit(fromCommitSha, filePath);
                var newVersionContent = _gitService.GetFileContentFromCommit(toCommitSha, filePath);

                if (oldVersionContent != null && newVersionContent != null)
                {
                    await File.WriteAllBytesAsync(oldVersionPath, oldVersionContent);
                    await File.WriteAllBytesAsync(newVersionPath, newVersionContent);

                    // 使用AutoCAD開啟兩個版本進行比較
                    await _autocadService.OpenDrawingsForComparisonAsync(oldVersionPath, newVersionPath);
                    
                    StatusMessage = $"已在AutoCAD中開啟圖面比較：\n舊版本: {oldVersionPath}\n新版本: {newVersionPath}";
                }
                else
                {
                    StatusMessage = "無法從Git提交中提取檔案內容";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"開啟圖面比較失敗: {ex.Message}";
            }
        }

        public async Task OpenDrawingAsync(string filePath)
        {
            try
            {
                // 檢查AutoCAD是否可用
                if (!_autocadService.IsAutoCADAvailable())
                {
                    StatusMessage = "AutoCAD未找到，請先設定AutoCAD路徑";
                    return;
                }

                var fullPath = Path.Combine(RepositoryPath, filePath);
                if (File.Exists(fullPath))
                {
                    // 強制使用AutoCAD開啟，而不是系統預設程式
                    await _autocadService.OpenDrawingAsync(fullPath);
                    StatusMessage = $"已使用AutoCAD開啟圖面: {filePath}";
                }
                else
                {
                    StatusMessage = $"檔案不存在: {fullPath}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"開啟圖面失敗: {ex.Message}";
            }
        }

        private async Task SetAutoCADPathAsync()
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".exe");
                picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                    var file = await picker.PickSingleFileAsync();
                    if (file != null)
                    {
                        _autocadService.SetAutoCADPath(file.Path);
                        StatusMessage = $"AutoCAD路徑已設定: {file.Path}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"設定AutoCAD路徑失敗: {ex.Message}";
            }
        }

        private void ForceRefresh()
        {
            try
            {
                StatusMessage = "正在強制重新整理...";
                _gitService.RefreshRepository();
                RefreshData();
                StatusMessage = "重新整理完成";
            }
            catch (Exception ex)
            {
                StatusMessage = $"強制重新整理失敗: {ex.Message}";
            }
        }

        private void CheckCadFileStatus()
        {
            try
            {
                StatusMessage = "正在檢查CAD檔案狀態...";
                
                var cadFiles = _gitService.GetCadFileChanges();
                var lockedFiles = cadFiles.Where(f => f.IsLocked).ToList();
                
                if (lockedFiles.Any())
                {
                    StatusMessage = $"發現 {lockedFiles.Count} 個CAD檔案被鎖定，可能正在AutoCAD中使用";
                }
                else if (cadFiles.Any())
                {
                    StatusMessage = $"發現 {cadFiles.Count()} 個CAD檔案變更，所有檔案都可以暫存";
                }
                else
                {
                    StatusMessage = "沒有發現CAD檔案變更";
                }

                // 檢查.gitignore問題
                if (_gitService.IsIgnoringCadFiles())
                {
                    StatusMessage += " (警告: .gitignore可能忽略了CAD檔案)";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"檢查CAD檔案狀態失敗: {ex.Message}";
            }
        }

        private void DiagnoseCommitIssues()
        {
            try
            {
                StatusMessage = "正在診斷提交問題...";
                
                var statusInfo = _gitService.GetDetailedStatus();
                
                if (!statusInfo.IsValid)
                {
                    StatusMessage = $"儲存庫狀態檢查失敗: {statusInfo.Message}";
                    return;
                }

                var diagnosis = new List<string>();
                
                // 檢查基本提交條件
                if (string.IsNullOrWhiteSpace(CommitMessage))
                {
                    diagnosis.Add("? 缺少提交訊息");
                }
                else
                {
                    diagnosis.Add("? 提交訊息已填寫");
                }

                if (string.IsNullOrWhiteSpace(AuthorName))
                {
                    diagnosis.Add("? 缺少作者姓名");
                }
                else
                {
                    diagnosis.Add("? 作者姓名已設定");
                }

                if (string.IsNullOrWhiteSpace(AuthorEmail))
                {
                    diagnosis.Add("? 缺少作者Email");
                }
                else
                {
                    diagnosis.Add("? 作者Email已設定");
                }

                // 檢查檔案狀態
                if (statusInfo.StagedFilesCount == 0)
                {
                    diagnosis.Add("? 沒有暫存的檔案");
                    
                    if (statusInfo.UnstagedFilesCount > 0)
                    {
                        diagnosis.Add($"?? 有 {statusInfo.UnstagedFilesCount} 個檔案變更未暫存");
                        diagnosis.Add("建議：點擊「暫存所有」按鈕");
                    }
                    else if (statusInfo.UntrackedFilesCount > 0)
                    {
                        diagnosis.Add($"?? 有 {statusInfo.UntrackedFilesCount} 個未追蹤的檔案");
                        diagnosis.Add("建議：點擊「暫存所有」按鈕");
                    }
                    else
                    {
                        diagnosis.Add("?? 工作目錄乾淨，沒有變更");
                        diagnosis.Add("建議：修改一些檔案後再嘗試提交");
                    }
                }
                else
                {
                    diagnosis.Add($"? 有 {statusInfo.StagedFilesCount} 個檔案已暫存，可以提交");
                }

                // 檢查CAD檔案特殊情況
                var cadFiles = _gitService.GetCadFileChanges();
                var lockedCadFiles = cadFiles.Where(f => f.IsLocked).ToList();
                
                if (lockedCadFiles.Any())
                {
                    diagnosis.Add($"?? 有 {lockedCadFiles.Count} 個CAD檔案被鎖定");
                    diagnosis.Add("建議：關閉AutoCAD後點擊「強制重新整理」");
                }

                // 檢查.gitignore問題
                if (_gitService.IsIgnoringCadFiles())
                {
                    diagnosis.Add("?? .gitignore可能忽略了CAD檔案");
                    diagnosis.Add("建議：檢查.gitignore設定");
                }

                var diagnosisText = string.Join("\n", diagnosis);
                StatusMessage = $"診斷結果:\n{diagnosisText}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"診斷失敗: {ex.Message}";
            }
        }

        private async Task EditUserSettingsAsync()
        {
            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    // 創建簡單的設定對話框
                    var dialog = new ContentDialog
                    {
                        Title = "編輯用戶設定",
                        PrimaryButtonText = "儲存",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = mainWindow.Content.XamlRoot
                    };

                    // 創建設定內容
                    var panel = new StackPanel { Spacing = 12 };
                    
                    var nameTextBox = new TextBox 
                    { 
                        Header = "作者姓名",
                        Text = AuthorName,
                        PlaceholderText = "請輸入您的姓名"
                    };
                    
                    var emailTextBox = new TextBox 
                    { 
                        Header = "作者信箱",
                        Text = AuthorEmail,
                        PlaceholderText = "請輸入您的Email地址"
                    };

                    panel.Children.Add(nameTextBox);
                    panel.Children.Add(emailTextBox);
                    
                    dialog.Content = panel;

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        var newName = nameTextBox.Text?.Trim();
                        var newEmail = emailTextBox.Text?.Trim();

                        if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(newEmail))
                        {
                            // 更新當前的作者資訊
                            AuthorName = newName;
                            AuthorEmail = newEmail;

                            // 儲存設定
                            var newSettings = new UserSettings
                            {
                                AuthorName = newName,
                                AuthorEmail = newEmail,
                                LastUpdated = System.DateTime.Now
                            };

                            _userSettingsService.SaveSettings(newSettings);
                            StatusMessage = "用戶設定已更新";
                        }
                        else
                        {
                            StatusMessage = "請輸入有效的作者姓名和信箱";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"編輯用戶設定失敗: {ex.Message}";
            }
        }

        #endregion

        private async Task RevertToCommitAsync()
        {
            if (SelectedCommit == null)
            {
                StatusMessage = "請選擇一個要回復的提交";
                return;
            }

            // 在開始時保存選中的提交資訊，避免在執行過程中丟失
            var targetCommit = SelectedCommit;

            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "確認回復版本",
                        Content = $"即將回復到以下版本：\n\n" +
                                 $"提交: {targetCommit.ShortSha}\n" +
                                 $"訊息: {targetCommit.Message.Split('\n')[0]}\n" +
                                 $"作者: {targetCommit.Author}\n" +
                                 $"日期: {targetCommit.Date:yyyy-MM-dd HH:mm}\n\n" +
                                 $"這將會創建一個新的提交來撤銷指定版本之後的所有變更。\n" +
                                 $"原有的提交歷史會被保留，這是安全的操作。\n\n" +
                                 $"是否確認執行？",
                        PrimaryButtonText = "確認回復",
                        SecondaryButtonText = "取消",
                        DefaultButton = ContentDialogButton.Secondary,
                        XamlRoot = mainWindow.Content.XamlRoot
                    };

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        StatusMessage = "正在回復版本...";
                        
                        // 執行Git revert操作
                        _gitService.RevertToCommit(targetCommit.Sha, AuthorName, AuthorEmail);
                        
                        RefreshData();
                        StatusMessage = $"成功回復到版本 {targetCommit.ShortSha}";
                    }
                    else
                    {
                        StatusMessage = "已取消回復操作";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"回復版本失敗: {ex.Message}";
            }
        }

        private async Task ResetToCommitAsync()
        {
            if (SelectedCommit == null)
            {
                StatusMessage = "請選擇一個要重置的提交";
                return;
            }

            // 在開始時保存選中的提交資訊，避免在執行過程中丟失
            var targetCommit = SelectedCommit;

            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "?? 危險操作：重置到指定版本",
                        Content = $"警告：此操作將會：\n\n" +
                                 $"1. 刪除指定提交之後的所有提交歷史\n" +
                                 $"2. 將工作目錄重置到指定版本\n" +
                                 $"3. 這是不可逆的操作！\n\n" +
                                 $"目標版本：\n" +
                                 $"提交: {targetCommit.ShortSha}\n" +
                                 $"訊息: {targetCommit.Message.Split('\n')[0]}\n" +
                                 $"作者: {targetCommit.Author}\n" +
                                 $"日期: {targetCommit.Date:yyyy-MM-dd HH:mm}\n\n" +
                                 $"建議：如果不確定，請使用「回復版本」而不是重置。\n\n" +
                                 $"確定要執行重置操作嗎？",
                        PrimaryButtonText = "確認重置",
                        SecondaryButtonText = "取消",
                        DefaultButton = ContentDialogButton.Secondary,
                        XamlRoot = mainWindow.Content.XamlRoot
                    };

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        // 再次確認
                        var confirmDialog = new ContentDialog
                        {
                            Title = "最後確認",
                            Content = "此操作將永久刪除提交歷史！\n\n請輸入 \"CONFIRM\" 以確認執行：",
                            PrimaryButtonText = "執行重置",
                            SecondaryButtonText = "取消",
                            DefaultButton = ContentDialogButton.Secondary,
                            XamlRoot = mainWindow.Content.XamlRoot
                        };

                        var confirmTextBox = new TextBox { PlaceholderText = "輸入 CONFIRM" };
                        confirmDialog.Content = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock { Text = "此操作將永久刪除提交歷史！\n\n請輸入 \"CONFIRM\" 以確認執行：", TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap },
                                confirmTextBox
                            },
                            Spacing = 12
                        };

                        var confirmResult = await confirmDialog.ShowAsync();

                        if (confirmResult == ContentDialogResult.Primary && 
                            confirmTextBox.Text?.Trim().ToUpper() == "CONFIRM")
                        {
                            StatusMessage = "正在重置版本...";
                            
                            // 執行Git reset --hard操作
                            _gitService.ResetToCommit(targetCommit.Sha);
                            
                            RefreshData();
                            StatusMessage = $"成功重置到版本 {targetCommit.ShortSha}";
                        }
                        else
                        {
                            StatusMessage = "已取消重置操作";
                        }
                    }
                    else
                    {
                        StatusMessage = "已取消重置操作";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"重置版本失敗: {ex.Message}";
            }
        }

        private void UpdateStatus()
        {
            if (_autocadService.IsAutoCADAvailable())
            {
                StatusMessage = "準備就緒 - AutoCAD已找到";
            }
            else
            {
                StatusMessage = "警告: 未找到AutoCAD，請設定AutoCAD路徑";
            }
        }

        private void UpdateCommitButtonTooltip()
        {
            var issues = new List<string>();

            if (!IsRepositoryLoaded)
            {
                issues.Add("尚未選擇有效的Git儲存庫");
            }

            if (string.IsNullOrWhiteSpace(CommitMessage))
            {
                issues.Add("請輸入提交訊息");
            }

            if (string.IsNullOrWhiteSpace(AuthorName))
            {
                issues.Add("作者姓名未設定");
            }

            if (string.IsNullOrWhiteSpace(AuthorEmail))
            {
                issues.Add("作者信箱未設定");
            }

            if (IsRepositoryLoaded)
            {
                try
                {
                    var statusInfo = _gitService.GetDetailedStatus();
                    if (!statusInfo.CanCommit && statusInfo.StagedFilesCount == 0)
                    {
                        if (statusInfo.UnstagedFilesCount > 0)
                        {
                            issues.Add("有檔案變更未暫存，請先點擊「暫存所有」");
                        }
                        else
                        {
                            issues.Add("沒有檔案變更可以提交");
                        }
                    }
                }
                catch (Exception ex)
                {
                    issues.Add($"檢查儲存庫狀態時發生錯誤: {ex.Message}");
                }
            }

            if (issues.Any())
            {
                CommitButtonTooltip = "無法提交: " + string.Join("、", issues);
            }
            else
            {
                CommitButtonTooltip = "點擊提交變更到Git儲存庫";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 簡單的RelayCommand實作
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}