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

            // �ϥζǤJ���Τ�]�w
            AuthorName = userSettings.AuthorName;
            AuthorEmail = userSettings.AuthorEmail;
            
            // �p�G���W�����x�s�w���|�A�۰ʸ��J
            if (!string.IsNullOrEmpty(userSettings.LastRepositoryPath) && 
                Directory.Exists(userSettings.LastRepositoryPath))
            {
                RepositoryPath = userSettings.LastRepositoryPath;
            }

            // �p�G��AutoCAD���|�]�w�A�۰ʮM��
            if (!string.IsNullOrEmpty(userSettings.AutoCADPath) && 
                File.Exists(userSettings.AutoCADPath))
            {
                _autocadService.SetAutoCADPath(userSettings.AutoCADPath);
            }

            // ��l�ƩR�O
            InitializeCommands();
            
            // ��s���A
            UpdateStatus();
            
            // ��l�ƴ�����s����
            UpdateCommitButtonTooltip();
        }

        // �O�d�즳���L�Ѽƺc�y��ƥH���V��ݮe�ʰ��D
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
            
            // �s�W��DWG�����R�O
            ForceRefreshCommand = new RelayCommand(ForceRefresh);
            CheckCadFilesCommand = new RelayCommand(CheckCadFileStatus);
            DiagnoseCommitCommand = new RelayCommand(DiagnoseCommitIssues);
            EditUserSettingsCommand = new RelayCommand(async () => await EditUserSettingsAsync());
            
            // �s�W�����^�_�R�O
            RevertToCommitCommand = new RelayCommand(async () => await RevertToCommitAsync(), CanRevertToCommit);
            ResetToCommitCommand = new RelayCommand(async () => await ResetToCommitAsync(), CanResetToCommit);
            
            // �s�W�ֳt����R�O
            QuickCommitCommand = new RelayCommand(QuickCommit, CanQuickCommit);
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
                
                // �O�s��Τ�]�w
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
                if (_commitMessage != newValue)  // �K�[�Ȥ��
                {
                    _commitMessage = newValue;
                    OnPropertyChanged();
                    
                    try
                    {
                        UpdateCommitButtonTooltip();
                        ((RelayCommand)CommitCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)QuickCommitCommand).RaiseCanExecuteChanged();
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
                if (_selectedCommit != value)  // �K�[�Ȥ���קK�����n����s
                {
                    _selectedCommit = value;
                    OnPropertyChanged();
                    
                    // �ϥ� try-catch �O�@ RaiseCanExecuteChanged �ե�
                    try
                    {
                        ((RelayCommand)CompareCommitsCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)RevertToCommitCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)ResetToCommitCommand).RaiseCanExecuteChanged();
                    }
                    catch (Exception ex)
                    {
                        // �O�����`�������s�ߥX�A�קK�Y��
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
        
        // �s�W��DWG�����R�O
        public ICommand ForceRefreshCommand { get; private set; }
        public ICommand CheckCadFilesCommand { get; private set; }
        public ICommand DiagnoseCommitCommand { get; private set; }
        public ICommand EditUserSettingsCommand { get; private set; }
        
        // �s�W�����^�_�R�O
        public ICommand RevertToCommitCommand { get; private set; }
        public ICommand ResetToCommitCommand { get; private set; }
        
        // �s�W�ֳt����R�O
        public ICommand QuickCommitCommand { get; private set; }

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
                        StatusMessage = $"�w����x�s�w: {folder.Path}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"��ܸ�Ƨ�����: {ex.Message}";
            }
        }

        private void InitializeRepository()
        {
            if (string.IsNullOrEmpty(RepositoryPath))
            {
                StatusMessage = "�Х�����x�s�w��Ƨ�";
                return;
            }

            try
            {
                _gitService.InitializeRepository();
                RefreshData();
                StatusMessage = "�x�s�w��l�Ʀ��\";
            }
            catch (Exception ex)
            {
                StatusMessage = $"��l���x�s�w����: {ex.Message}";
            }
        }

        private void RefreshData()
        {
            if (string.IsNullOrEmpty(RepositoryPath))
                return;

            try
            {
                // �j��s��z�x�s�w���A
                _gitService.RefreshRepository();
                
                IsRepositoryLoaded = _gitService.IsRepositoryValid();
                
                if (IsRepositoryLoaded)
                {
                    CurrentBranch = _gitService.GetCurrentBranch();
                    
                    // ��s����C��
                    Branches.Clear();
                    foreach (var branch in _gitService.GetBranches())
                    {
                        Branches.Add(branch);
                    }

                    // ��s������v
                    Commits.Clear();
                    foreach (var commit in _gitService.GetCommitHistory())
                    {
                        Commits.Add(commit);
                    }

                    // ��s���A - �u�����CAD�ɮת��A
                    StatusItems.Clear();
                    
                    // �����CAD�ɮת��ԲӪ��A
                    var cadFileChanges = _gitService.GetCadFileChanges();
                    foreach (var cadFile in cadFileChanges)
                    {
                        var statusText = $"{cadFile.State}: {cadFile.FilePath}";
                        if (cadFile.IsLocked)
                        {
                            statusText += " (�ɮרϥΤ�)";
                        }
                        statusText += $" ({FormatFileSize(cadFile.FileSize)})";
                        StatusItems.Add(statusText);
                    }
                    
                    // �A��ܨ�L�ɮת����A
                    foreach (var status in _gitService.GetStatusChanges())
                    {
                        // �קK�������CAD�ɮ�
                        if (!StatusItems.Any(s => s.Contains(status.Split(':')[1].Trim())))
                        {
                            StatusItems.Add(status);
                        }
                    }

                    // �ˬd�O�_��.gitignore���D
                    if (_gitService.IsIgnoringCadFiles())
                    {
                        StatusMessage = $"�x�s�w�w���J - ����: {CurrentBranch} (�`�N: .gitignore�i�੿���FCAD�ɮ�)";
                    }
                    else
                    {
                        StatusMessage = $"�x�s�w�w���J - ����: {CurrentBranch}";
                    }

                    // ��s������s����
                    UpdateCommitButtonTooltip();
                }
                else
                {
                    StatusMessage = "�ҿ��Ƨ����O���Ī�Git�x�s�w";
                }

                // ��s������s����
                UpdateCommitButtonTooltip();
            }
            catch (Exception ex)
            {
                StatusMessage = $"���J�x�s�w����: {ex.Message}";
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
                    
                    // �p�G�����L���ɮסA���X�ԲӸ�T
                    if (result.SkippedCount > 0)
                    {
                        StatusMessage += $"\n���L���ɮ�: {string.Join(", ", result.SkippedFiles)}";
                    }
                }
                else
                {
                    StatusMessage = result.Message;
                }

                // ��s������s����
                UpdateCommitButtonTooltip();
            }
            catch (Exception ex)
            {
                StatusMessage = $"�Ȧs�ܧ󥢱�: {ex.Message}";
                UpdateCommitButtonTooltip();
            }
        }

        private void CommitChanges()
        {
            try
            {
                // ���ˬd�x�s�w���A
                var statusInfo = _gitService.GetDetailedStatus();
                
                if (!statusInfo.CanCommit)
                {
                    StatusMessage = $"�L�k����: {statusInfo.Message}";
                    return;
                }

                _gitService.Commit(CommitMessage, AuthorName, AuthorEmail);
                CommitMessage = string.Empty;
                RefreshData();
                StatusMessage = "���榨�\";
            }
            catch (Exception ex)
            {
                StatusMessage = $"���楢��: {ex.Message}";
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

        private bool CanQuickCommit()
        {
            // �u�n���x�s�w���J�B�@�̸�T����N�i�H�ֳt����
            if (!IsRepositoryLoaded || 
                string.IsNullOrWhiteSpace(AuthorName) || 
                string.IsNullOrWhiteSpace(AuthorEmail))
            {
                return false;
            }

            try
            {
                var statusInfo = _gitService.GetDetailedStatus();
                // �u�n�����Ȧs���ܧ�Υ��l�ܪ��ɮ״N�i�H�ֳt����
                return statusInfo.UnstagedFilesCount > 0 || statusInfo.UntrackedFilesCount > 0;
            }
            catch
            {
                return false;
            }
        }

        private void QuickCommit()
        {
            try
            {
                // �ˬd�O�_������T���A�p�G�S���h�ϥιw�]�T��
                var message = string.IsNullOrWhiteSpace(CommitMessage) 
                    ? $"�ֳt���� - {DateTime.Now:yyyy-MM-dd HH:mm}" 
                    : CommitMessage;

                // �����ռȦs�Ҧ��ܧ�
                var stageResult = _gitService.StageAllChangesWithResult();
                
                if (!stageResult.Success)
                {
                    StatusMessage = $"�Ȧs����: {stageResult.Message}";
                    return;
                }

                // �p�G�Ȧs���\�A�~�򴣥�
                if (stageResult.StagedCount > 0)
                {
                    _gitService.Commit(message, AuthorName, AuthorEmail);
                    CommitMessage = string.Empty;
                    RefreshData();
                    
                    var successMessage = $"�ֳt���榨�\�I�w���� {stageResult.StagedCount} ���ɮ�";
                    if (stageResult.SkippedCount > 0)
                    {
                        successMessage += $"�A���L {stageResult.SkippedCount} ���ɮ�";
                    }
                    StatusMessage = successMessage;
                }
                else
                {
                    StatusMessage = "�S���ɮ׻ݭn����";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�ֳt���楢��: {ex.Message}";
            }
        }

        private bool CanCompareCommits()
        {
            try
            {
                // �����ϥΨp���r�q�קKĲ�o�ݩʳX��
                return _selectedCommit != null && _isRepositoryLoaded;
            }
            catch
            {
                // �p�G�o�ͥ��󲧱`�A��^ false
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
                        Title = "�إ߷s����",
                        Content = new TextBox { PlaceholderText = "��J����W��" },
                        PrimaryButtonText = "�إ�",
                        CloseButtonText = "����",
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
                            StatusMessage = $"���� '{branchName}' �w�إ�";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�إߤ��䥢��: {ex.Message}";
            }
        }

        private void CheckoutBranch(string? branchName)
        {
            if (string.IsNullOrEmpty(branchName)) return;

            try
            {
                _gitService.CheckoutBranch(branchName);
                RefreshData();
                StatusMessage = $"�w�����ܤ���: {branchName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"�������䥢��: {ex.Message}";
            }
        }

        private async Task CompareCommitsAsync()
        {
            if (SelectedCommit == null)
            {
                StatusMessage = "�п�ܤ@�Ӵ���i����";
                return;
            }

            // �b�}�l�ɫO�s�襤�������T�A�קK�b����L�{���ᥢ
            var targetCommit = SelectedCommit;

            // �ˬdAutoCAD�O�_�i��
            if (!_autocadService.IsAutoCADAvailable())
            {
                StatusMessage = "AutoCAD�����A�L�k�i��ϭ�����C���I���u�]�wAutoCAD���|�v���s�]�wAutoCAD���w�˸��|�C";
                return;
            }

            try
            {
                StatusMessage = "���b���R�����ܧ�...";
                
                // ²�ơG�P�e�@�Ӵ�����
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
                        StatusMessage = $"��� {ChangedFiles.Count} ���ܧ󪺹ϭ��ɮסC���b�ǳ�AutoCAD���...";
                        
                        // ��ܴ��ܹ�ܮ�
                        var mainWindow = ((App)App.Current).MainWindow;
                        if (mainWindow != null)
                        {
                            var dialog = new ContentDialog
                            {
                                Title = "�ϭ��������",
                                Content = $"�Y�N�ϥ�AutoCAD�}�ҥH�U�ɮ׶i�����G\n\n" +
                                         $"�ɮסG{ChangedFiles.First()}\n" +
                                         $"�ª����G{previousCommit.ShortSha} ({previousCommit.Message.Split('\n')[0]})\n" +
                                         $"�s�����G{targetCommit.ShortSha} ({targetCommit.Message.Split('\n')[0]})\n\n" +
                                         $"AutoCAD�N�}�Ҩ�Ӫ������ɮרѱz����C",
                                PrimaryButtonText = "�}�l���",
                                SecondaryButtonText = "����",
                                DefaultButton = ContentDialogButton.Primary,
                                XamlRoot = mainWindow.Content.XamlRoot
                            };

                            var result = await dialog.ShowAsync();
                            
                            if (result == ContentDialogResult.Primary)
                            {
                                StatusMessage = "���b�ϥ�AutoCAD�}�ҹϭ��i����...";
                                await OpenDrawingForComparisonAsync(ChangedFiles.First(), previousCommit.Sha, targetCommit.Sha);
                            }
                            else
                            {
                                StatusMessage = "�w�����ϭ����";
                            }
                        }
                    }
                    else
                    {
                        StatusMessage = "�b��ܪ����椧���S�����ϭ��ɮ��ܧ�";
                    }
                }
                else
                {
                    StatusMessage = "��ܪ�����S���e�@�Ӵ���i����]�o�O�Ĥ@�Ӵ���^";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"������楢��: {ex.Message}";
            }
        }

        private async Task OpenDrawingForComparisonAsync(string filePath, string fromCommitSha, string toCommitSha)
        {
            try
            {
                // �ϥ�Git checkout�Ыب�Ӫ������ɮ�
                var tempDir = Path.GetTempPath();
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var comparisonDir = Path.Combine(tempDir, $"GitDWG_Compare_{timestamp}");
                Directory.CreateDirectory(comparisonDir);

                var fileName = Path.GetFileName(filePath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                var fileExt = Path.GetExtension(filePath);

                var oldVersionPath = Path.Combine(comparisonDir, $"{fileNameWithoutExt}_�ª���_{fromCommitSha.Substring(0, 7)}{fileExt}");
                var newVersionPath = Path.Combine(comparisonDir, $"{fileNameWithoutExt}_�s����_{toCommitSha.Substring(0, 7)}{fileExt}");

                // �qGit������Ӫ������ɮ�
                var oldVersionContent = _gitService.GetFileContentFromCommit(fromCommitSha, filePath);
                var newVersionContent = _gitService.GetFileContentFromCommit(toCommitSha, filePath);

                if (oldVersionContent != null && newVersionContent != null)
                {
                    await File.WriteAllBytesAsync(oldVersionPath, oldVersionContent);
                    await File.WriteAllBytesAsync(newVersionPath, newVersionContent);

                    // �ϥ�AutoCAD�}�Ҩ�Ӫ����i����
                    await _autocadService.OpenDrawingsForComparisonAsync(oldVersionPath, newVersionPath);
                    
                    StatusMessage = $"�w�bAutoCAD���}�ҹϭ�����G\n�ª���: {oldVersionPath}\n�s����: {newVersionPath}";
                }
                else
                {
                    StatusMessage = "�L�k�qGit���椤�����ɮפ��e";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�}�ҹϭ��������: {ex.Message}";
            }
        }

        public async Task OpenDrawingAsync(string filePath)
        {
            try
            {
                // �ˬdAutoCAD�O�_�i��
                if (!_autocadService.IsAutoCADAvailable())
                {
                    StatusMessage = "AutoCAD�����A�Х��]�wAutoCAD���|";
                    return;
                }

                var fullPath = Path.Combine(RepositoryPath, filePath);
                if (File.Exists(fullPath))
                {
                    // �j��ϥ�AutoCAD�}�ҡA�Ӥ��O�t�ιw�]�{��
                    await _autocadService.OpenDrawingAsync(fullPath);
                    StatusMessage = $"�w�ϥ�AutoCAD�}�ҹϭ�: {filePath}";
                }
                else
                {
                    StatusMessage = $"�ɮפ��s�b: {fullPath}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�}�ҹϭ�����: {ex.Message}";
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
                        StatusMessage = $"AutoCAD���|�w�]�w: {file.Path}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�]�wAutoCAD���|����: {ex.Message}";
            }
        }

        private void ForceRefresh()
        {
            try
            {
                StatusMessage = "���b�j��s��z...";
                _gitService.RefreshRepository();
                RefreshData();
                StatusMessage = "���s��z����";
            }
            catch (Exception ex)
            {
                StatusMessage = $"�j��s��z����: {ex.Message}";
            }
        }

        private void CheckCadFileStatus()
        {
            try
            {
                StatusMessage = "���b�ˬdCAD�ɮת��A...";
                
                var cadFiles = _gitService.GetCadFileChanges();
                var lockedFiles = cadFiles.Where(f => f.IsLocked).ToList();
                
                if (lockedFiles.Any())
                {
                    StatusMessage = $"�o�{ {lockedFiles.Count} ��CAD�ɮ׳Q��w�A�i�ॿ�bAutoCAD���ϥ�";
                }
                else if (cadFiles.Any())
                {
                    StatusMessage = $"�o�{ {cadFiles.Count()} ��CAD�ɮ��ܧ�A�Ҧ��ɮ׳��i�H�Ȧs";
                }
                else
                {
                    StatusMessage = "�S���o�{CAD�ɮ��ܧ�";
                }

                // �ˬd.gitignore���D
                if (_gitService.IsIgnoringCadFiles())
                {
                    StatusMessage += " (ĵ�i: .gitignore�i�੿���FCAD�ɮ�)";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�ˬdCAD�ɮת��A����: {ex.Message}";
            }
        }

        private void DiagnoseCommitIssues()
        {
            try
            {
                StatusMessage = "���b�E�_������D...";
                
                var statusInfo = _gitService.GetDetailedStatus();
                
                if (!statusInfo.IsValid)
                {
                    StatusMessage = $"�x�s�w���A�ˬd����: {statusInfo.Message}";
                    return;
                }

                var diagnosis = new List<string>();
                
                // �ˬd�򥻴������
                if (string.IsNullOrWhiteSpace(CommitMessage))
                {
                    diagnosis.Add("? �ʤִ���T��");
                }
                else
                {
                    diagnosis.Add("? ����T���w��g");
                }

                if (string.IsNullOrWhiteSpace(AuthorName))
                {
                    diagnosis.Add("? �ʤ֧@�̩m�W");
                }
                else
                {
                    diagnosis.Add("? �@�̩m�W�w�]�w");
                }

                if (string.IsNullOrWhiteSpace(AuthorEmail))
                {
                    diagnosis.Add("? �ʤ֧@��Email");
                }
                else
                {
                    diagnosis.Add("? �@��Email�w�]�w");
                }

                // �ˬd�ɮת��A
                if (statusInfo.StagedFilesCount == 0)
                {
                    diagnosis.Add("? �S���Ȧs���ɮ�");
                    
                    if (statusInfo.UnstagedFilesCount > 0)
                    {
                        diagnosis.Add($"?? �� {statusInfo.UnstagedFilesCount} ���ɮ��ܧ󥼼Ȧs");
                        diagnosis.Add("��ĳ�G�I���u�Ȧs�Ҧ��v���s");
                    }
                    else if (statusInfo.UntrackedFilesCount > 0)
                    {
                        diagnosis.Add($"?? �� {statusInfo.UntrackedFilesCount} �ӥ��l�ܪ��ɮ�");
                        diagnosis.Add("��ĳ�G�I���u�Ȧs�Ҧ��v���s");
                    }
                    else
                    {
                        diagnosis.Add("?? �u�@�ؿ����b�A�S���ܧ�");
                        diagnosis.Add("��ĳ�G�ק�@���ɮ׫�A���մ���");
                    }
                }
                else
                {
                    diagnosis.Add($"? �� {statusInfo.StagedFilesCount} ���ɮפw�Ȧs�A�i�H����");
                }

                // �ˬdCAD�ɮׯS���p
                var cadFiles = _gitService.GetCadFileChanges();
                var lockedCadFiles = cadFiles.Where(f => f.IsLocked).ToList();
                
                if (lockedCadFiles.Any())
                {
                    diagnosis.Add($"?? �� {lockedCadFiles.Count} ��CAD�ɮ׳Q��w");
                    diagnosis.Add("��ĳ�G����AutoCAD���I���u�j��s��z�v");
                }

                // �ˬd.gitignore���D
                if (_gitService.IsIgnoringCadFiles())
                {
                    diagnosis.Add("?? .gitignore�i�੿���FCAD�ɮ�");
                    diagnosis.Add("��ĳ�G�ˬd.gitignore�]�w");
                }

                // �ˬd�Τ�]�w�ɮת��A
                try
                {
                    var fileInfo = _userSettingsService.GetType()
                        .GetMethod("GetSettingsFileInfo")?
                        .Invoke(_userSettingsService, null) as dynamic;
                    
                    if (fileInfo != null)
                    {
                        diagnosis.Add($"?? �Τ�]�w�ɮת��A�G");
                        diagnosis.Add($"   - �ɮצs�b: {fileInfo.Exists}");
                        diagnosis.Add($"   - �ؿ��s�b: {fileInfo.DirectoryExists}");
                        diagnosis.Add($"   - �g�J�v��: {fileInfo.HasWritePermission}");
                        
                        if (!fileInfo.HasWritePermission)
                        {
                            diagnosis.Add("?? �Τ�]�w�ɮ׵L�g�J�v��");
                            diagnosis.Add("��ĳ�G�H�޲z������������ˬd�ɮ��v��");
                        }
                    }
                }
                catch (Exception settingsEx)
                {
                    diagnosis.Add($"?? �ˬd�Τ�]�w�ɵo�Ϳ��~: {settingsEx.Message}");
                }

                var diagnosisText = string.Join("\n", diagnosis);
                StatusMessage = $"�E�_���G:\n{diagnosisText}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"�E�_����: {ex.Message}";
            }
        }

        private async Task EditUserSettingsAsync()
        {
            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    // �Ы�²�檺�]�w��ܮ�
                    var dialog = new ContentDialog
                    {
                        Title = "�s��Τ�]�w",
                        PrimaryButtonText = "�x�s",
                        CloseButtonText = "����",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = mainWindow.Content.XamlRoot
                    };

                    // �Ыس]�w���e
                    var panel = new StackPanel { Spacing = 12 };
                    
                    var nameTextBox = new TextBox 
                    { 
                        Header = "�@�̩m�W",
                        Text = AuthorName,
                        PlaceholderText = "�п�J�z���m�W"
                    };
                    
                    var emailTextBox = new TextBox 
                    { 
                        Header = "�@�̫H�c",
                        Text = AuthorEmail,
                        PlaceholderText = "�п�J�z��Email�a�}"
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
                            // ��s��e���@�̸�T
                            AuthorName = newName;
                            AuthorEmail = newEmail;

                            // �x�s�]�w
                            var newSettings = new UserSettings
                            {
                                AuthorName = newName,
                                AuthorEmail = newEmail,
                                LastUpdated = System.DateTime.Now
                            };

                            _userSettingsService.SaveSettings(newSettings);
                            StatusMessage = "�Τ�]�w�w��s";
                        }
                        else
                        {
                            StatusMessage = "�п�J���Ī��@�̩m�W�M�H�c";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"�s��Τ�]�w����: {ex.Message}";
            }
        }

        private async Task RevertToCommitAsync()
        {
            if (SelectedCommit == null)
            {
                StatusMessage = "�п�ܤ@�ӭn�^�_������";
                return;
            }

            // �b�}�l�ɫO�s�襤�������T�A�קK�b����L�{���ᥢ
            var targetCommit = SelectedCommit;

            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    // �����e����M���A�H��
                    var currentBranch = _gitService.GetCurrentBranch();
                    var statusInfo = _gitService.GetDetailedStatus();
                    
                    // �ˬd�u�@�ؿ��O�_�������檺�ܧ�
                    if (statusInfo.UnstagedFilesCount > 0 || statusInfo.UntrackedFilesCount > 0)
                    {
                        var warningDialog = new ContentDialog
                        {
                            Title = "?? �u�@�ؿ��������檺�ܧ�",
                            Content = $"�˴���H�U�����檺�ܧ�G\n\n" +
                                     $"? ���Ȧs���ɮסG{statusInfo.UnstagedFilesCount} ��\n" +
                                     $"? ���l�ܪ��ɮסG{statusInfo.UntrackedFilesCount} ��\n\n" +
                                     $"��ĳ�b���檩���^�_�e������o���ܧ�C\n\n" +
                                     $"�z�i�H�G\n" +
                                     $"1. �����ާ@�A�������ܧ�\n" +
                                     $"2. �~�����]�ܧ�i��|�ᥢ�^",
                            PrimaryButtonText = "�����ާ@",
                            SecondaryButtonText = "�~�����",
                            DefaultButton = ContentDialogButton.Primary,
                            XamlRoot = mainWindow.Content.XamlRoot,
                            RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark
                        };

                        var warningResult = await warningDialog.ShowAsync();
                        if (warningResult == ContentDialogResult.Primary)
                        {
                            StatusMessage = "�w���������^�_�ާ@";
                            return;
                        }
                    }

                    // �ЫظԲӪ��T�{��ܮ�
                    var confirmDialog = new ContentDialog
                    {
                        Title = "?? �T�{�w���^�_����",
                        Content = $"�Y�N����w���^�_�ާ@�G\n\n" +
                                 $"?? �ؼЪ����H���G\n" +
                                 $"? ����G{targetCommit.ShortSha}\n" +
                                 $"? �T���G{targetCommit.Message.Split('\n')[0]}\n" +
                                 $"? �@�̡G{targetCommit.Author}\n" +
                                 $"? ����G{targetCommit.Date:yyyy-MM-dd HH:mm:ss}\n\n" +
                                 $"??? �w���^�_�ާ@�����G\n" +
                                 $"? �o�N�Ыؤ@�ӷs������ӺM�P���w�������᪺�Ҧ��ܧ�\n" +
                                 $"? �즳��������v�|�����O�d\n" +
                                 $"? �o�O�w���B�i�f���ާ@\n" +
                                 $"? �p�G�ݭn�A�z�i�H�A���^�_����󪩥�\n\n" +
                                 $"?? ��e���A�G\n" +
                                 $"? ����G{currentBranch}\n" +
                                 $"? �w�Ȧs�ɮסG{statusInfo.StagedFilesCount} ��\n" +
                                 $"? ���Ȧs�ɮסG{statusInfo.UnstagedFilesCount} ��\n\n" +
                                 $"�O�_�T�{����w���^�_�H",
                        PrimaryButtonText = "�T�{�^�_",
                        SecondaryButtonText = "����",
                        DefaultButton = ContentDialogButton.Secondary,
                        XamlRoot = mainWindow.Content.XamlRoot,
                        RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark
                    };

                    var result = await confirmDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        StatusMessage = "���b����w���^�_�ާ@...";
                        
                        // ����Git revert�ާ@
                        _gitService.RevertToCommit(targetCommit.Sha, AuthorName, AuthorEmail);
                        
                        RefreshData();
                        StatusMessage = $"? ���\�^�_�쪩�� {targetCommit.ShortSha}�A�w�Ыطs���^�_����";
                    }
                    else
                    {
                        StatusMessage = "�w���������^�_�ާ@";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"? �����^�_����: {ex.Message}";
            }
        }

        private async Task ResetToCommitAsync()
        {
            if (SelectedCommit == null)
            {
                StatusMessage = "�п�ܤ@�ӭn���m������";
                return;
            }

            // �b�}�l�ɫO�s�襤�������T�A�קK�b����L�{���ᥢ
            var targetCommit = SelectedCommit;

            try
            {
                var mainWindow = ((App)App.Current).MainWindow;
                if (mainWindow != null)
                {
                    // �����e����M���A�H��
                    var currentBranch = _gitService.GetCurrentBranch();
                    var statusInfo = _gitService.GetDetailedStatus();
                    
                    // �p��N�n�R��������ƶq
                    var commits = Commits.ToList();
                    var targetIndex = commits.IndexOf(targetCommit);
                    var commitsToDelete = targetIndex; // 0-based�A�ҥH targetIndex �N�O�n�R��������ƶq

                    // �Ĥ@�hĵ�i��ܮ�
                    var warningDialog = new ContentDialog
                    {
                        Title = "?? �M�I�ާ@ĵ�i",
                        Content = $"?? ���m�����O�M�I�ާ@�I\n\n" +
                                 $"���ާ@�N�|�G\n" +
                                 $"? �ä[�R�� {commitsToDelete} �Ӵ���Ψ���v�O��\n" +
                                 $"? �N�u�@�ؿ����m����w����\n" +
                                 $"? �L�k�_��w�R��������\n" +
                                 $"? �i��ɭP�ƾڥᥢ\n\n" +
                                 $"?? ��ĳ�ϥΡu�w���^�_�v���N�G\n" +
                                 $"? �w���^�_�|�O�d�Ҧ����v�O��\n" +
                                 $"? �Ыطs����ӺM�P�ܧ�\n" +
                                 $"? �ާ@�����i�f\n" +
                                 $"? �A�X�ζ���@����\n\n" +
                                 $"�z�T�w�n�~��ϥΦM�I�����m�ާ@�ܡH",
                        PrimaryButtonText = "�ڤF�ѭ��I�A�~��",
                        SecondaryButtonText = "�����ާ@",
                        DefaultButton = ContentDialogButton.Secondary,
                        XamlRoot = mainWindow.Content.XamlRoot,
                        RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark
                    };

                    var warningResult = await warningDialog.ShowAsync();

                    if (warningResult != ContentDialogResult.Primary)
                    {
                        StatusMessage = "�w�������m�ާ@";
                        return;
                    }

                    // �ĤG�h�T�{��ܮ�
                    var confirmDialog = new ContentDialog
                    {
                        Title = "?? �̲׽T�{",
                        PrimaryButtonText = "���歫�m",
                        SecondaryButtonText = "����",
                        DefaultButton = ContentDialogButton.Secondary,
                        XamlRoot = mainWindow.Content.XamlRoot,
                        RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark
                    };

                    // �ЫؽT�{��J���O
                    var confirmPanel = new StackPanel { Spacing = 16 };
                    
                    var finalWarning = new TextBlock
                    {
                        Text = "?? �o�O�̫�@���T�{���|�I\n\n" +
                               "���m�ާ@�N�ߧY����B�L�k�M�P�C\n" +
                               "�п�J�H�U�T�{��r�H�~��G",
                        FontSize = 12,
                        TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                    };

                    var confirmationText = new TextBlock
                    {
                        Text = "�T�{�R��������v",
                        FontSize = 14,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                        Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 0)
                    };

                    var confirmTextBox = new TextBox
                    {
                        PlaceholderText = "�Ч����J�W��T�{��r",
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                        FontSize = 12
                    };

                    // �ؼЪ����H��
                    var targetInfoText = new TextBlock
                    {
                        Text = $"?? ���m�ؼЪ����G\n\n" +
                               $"����G{targetCommit.ShortSha}\n" +
                               $"�T���G{targetCommit.Message.Split('\n')[0]}\n" +
                               $"�@�̡G{targetCommit.Author}\n" +
                               $"����G{targetCommit.Date:yyyy-MM-dd HH:mm:ss}\n\n" +
                               $"?? �N�R�����������᪺ {commitsToDelete} �Ӵ���",
                        FontSize = 11,
                        TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                    };

                    confirmPanel.Children.Add(finalWarning);
                    confirmPanel.Children.Add(confirmationText);
                    confirmPanel.Children.Add(confirmTextBox);
                    confirmPanel.Children.Add(targetInfoText);

                    confirmDialog.Content = confirmPanel;

                    var confirmResult = await confirmDialog.ShowAsync();

                    if (confirmResult == ContentDialogResult.Primary && 
                        confirmTextBox.Text?.Trim() == "�T�{�R��������v")
                    {
                        StatusMessage = "���b���歫�m�ާ@...";
                        
                        // ����Git reset --hard�ާ@
                        _gitService.ResetToCommit(targetCommit.Sha);
                        
                        RefreshData();
                        StatusMessage = $"? ���\���m�쪩�� {targetCommit.ShortSha}�A�w�R�� {commitsToDelete} �ӫ��򴣥�";
                    }
                    else
                    {
                        StatusMessage = "�w�������m�ާ@�]�T�{��r�����T�ΥΤ�����^";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"? ���m��������: {ex.Message}";
            }
        }

        #endregion

        private void UpdateStatus()
        {
            if (_autocadService.IsAutoCADAvailable())
            {
                StatusMessage = "�ǳƴN�� - AutoCAD�w���";
            }
            else
            {
                StatusMessage = "ĵ�i: �����AutoCAD�A�г]�wAutoCAD���|";
            }
        }

        private void UpdateCommitButtonTooltip()
        {
            try
            {
                var issues = new List<string>();

                if (!IsRepositoryLoaded)
                {
                    issues.Add("�|����ܦ��Ī�Git�x�s�w");
                }

                if (string.IsNullOrWhiteSpace(CommitMessage))
                {
                    issues.Add("�п�J����T��");
                }

                if (string.IsNullOrWhiteSpace(AuthorName))
                {
                    issues.Add("�@�̩m�W���]�w");
                }

                if (string.IsNullOrWhiteSpace(AuthorEmail))
                {
                    issues.Add("�@�̫H�c���]�w");
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
                                issues.Add("���ɮ��ܧ󥼼Ȧs�A�Х��I���u�Ȧs�Ҧ��v");
                            }
                            else
                            {
                                issues.Add("�S���ɮ��ܧ�i�H����");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        issues.Add($"�ˬd�x�s�w���A�ɵo�Ϳ��~: {ex.Message}");
                    }
                }

                if (issues.Any())
                {
                    CommitButtonTooltip = "�L�k����: " + string.Join("�B", issues);
                }
                else
                {
                    CommitButtonTooltip = "�I�������ܧ��Git�x�s�w";
                }
            }
            catch (Exception ex)
            {
                // �p�G��s���ܥ��ѡA�]�w�w�]��
                CommitButtonTooltip = "������s���A��s����";
                System.Diagnostics.Debug.WriteLine($"UpdateCommitButtonTooltip failed: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ²�檺RelayCommand��@
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