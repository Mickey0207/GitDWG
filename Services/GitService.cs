using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GitDWG.Services
{
    public class GitService : IDisposable
    {
        private string _repositoryPath;
        private Repository? _repository;
        private readonly SemaphoreSlim _repositoryLock = new(1, 1);
        private bool _disposed;

        public GitService(string repositoryPath)
        {
            _repositoryPath = repositoryPath ?? string.Empty;
            if (!string.IsNullOrEmpty(_repositoryPath) && Repository.IsValid(_repositoryPath))
            {
                _repository = new Repository(_repositoryPath);
            }
        }

        public bool IsRepositoryValid()
        {
            return !string.IsNullOrEmpty(_repositoryPath) && Repository.IsValid(_repositoryPath);
        }

        public void InitializeRepository()
        {
            if (!Directory.Exists(_repositoryPath))
            {
                Directory.CreateDirectory(_repositoryPath);
            }

            if (!Repository.IsValid(_repositoryPath))
            {
                Repository.Init(_repositoryPath);
                DisposeRepository();
                _repository = new Repository(_repositoryPath);
                CreateCadOptimizedGitFiles();
            }
        }

        private void CreateCadOptimizedGitFiles()
        {
            if (string.IsNullOrEmpty(_repositoryPath)) return;

            var gitignorePath = Path.Combine(_repositoryPath, ".gitignore");
            if (!File.Exists(gitignorePath))
            {
                var gitignoreContent = @"# AutoCAD暫存檔案
*.bak
*.dwl
*.dwl2
*.sv$
*.tmp
*.$$$
*.ac$
*.dwg.lock

# Windows系統檔案
Thumbs.db
Desktop.ini
.DS_Store

# 其他暫存檔案
*.log
*.temp
";
                File.WriteAllText(gitignorePath, gitignoreContent);
            }

            var gitattributesPath = Path.Combine(_repositoryPath, ".gitattributes");
            if (!File.Exists(gitattributesPath))
            {
                var gitattributesContent = @"# CAD檔案設定為二進制檔案
*.dwg binary
*.dxf text
*.dwt binary
*.dws binary

# 其他二進制格式
*.pdf binary
*.jpg binary
*.png binary
*.gif binary
";
                File.WriteAllText(gitattributesPath, gitattributesContent);
            }
        }

        public void SetRepository(string repositoryPath)
        {
            _repositoryLock.Wait();
            try
            {
                _repositoryPath = repositoryPath ?? string.Empty;
                DisposeRepository();
                if (!string.IsNullOrEmpty(_repositoryPath) && Repository.IsValid(_repositoryPath))
                {
                    _repository = new Repository(_repositoryPath);
                }
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public IEnumerable<string> GetStatusChanges()
        {
            if (_repository == null) return Enumerable.Empty<string>();
            _repositoryLock.Wait();
            try
            {
                var status = _repository.RetrieveStatus();
                return status.Select(s => $"{s.State}: {s.FilePath}").ToList();
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public IEnumerable<CadFileStatus> GetCadFileChanges()
        {
            if (_repository == null) return Enumerable.Empty<CadFileStatus>();
            _repositoryLock.Wait();
            try
            {
                var status = _repository.RetrieveStatus();
                var cadFiles = status.Where(s =>
                    s.FilePath.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) ||
                    s.FilePath.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase) ||
                    s.FilePath.EndsWith(".dwt", StringComparison.OrdinalIgnoreCase) ||
                    s.FilePath.EndsWith(".dws", StringComparison.OrdinalIgnoreCase));

                var result = new List<CadFileStatus>();
                foreach (var s in cadFiles)
                {
                    var fullPath = Path.Combine(_repositoryPath, s.FilePath);
                    result.Add(new CadFileStatus
                    {
                        FilePath = s.FilePath,
                        State = s.State.ToString(),
                        FileSize = GetFileSize(fullPath),
                        LastModified = GetLastModified(fullPath),
                        IsLocked = IsFileLocked(fullPath)
                    });
                }
                return result;
            }
            catch (Exception)
            {
                return Enumerable.Empty<CadFileStatus>();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        private bool IsFileLocked(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        private long GetFileSize(string filePath)
        {
            try
            {
                return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
            }
            catch
            {
                return 0;
            }
        }

        private DateTime GetLastModified(string filePath)
        {
            try
            {
                return File.Exists(filePath) ? File.GetLastWriteTime(filePath) : DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public void RefreshRepository()
        {
            if (_repository == null) return;
            _repositoryLock.Wait();
            try
            {
                _repository.RetrieveStatus(new StatusOptions());
            }
            catch (Exception)
            {
                try
                {
                    DisposeRepository();
                    _repository = new Repository(_repositoryPath);
                }
                catch
                {
                    _repository = null;
                }
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public StageResult StageAllChangesWithResult()
        {
            if (_repository == null)
                return new StageResult { Success = false, Message = "Git儲存庫未初始化" };

            _repositoryLock.Wait();
            try
            {
                var status = _repository.RetrieveStatus();
                var filesToStage = status.Where(s => s.State.HasFlag(FileStatus.NewInWorkdir) ||
                                                s.State.HasFlag(FileStatus.ModifiedInWorkdir) ||
                                                s.State.HasFlag(FileStatus.DeletedFromWorkdir)).ToList();

                if (!filesToStage.Any())
                {
                    return new StageResult
                    {
                        Success = true,
                        Message = "沒有檔案需要暫存",
                        StagedCount = 0,
                        SkippedCount = 0
                    };
                }

                int stagedCount = 0;
                int skippedCount = 0;
                var skippedFiles = new List<string>();

                foreach (var item in filesToStage)
                {
                    var fullPath = Path.Combine(_repositoryPath, item.FilePath);
                    if (IsFileLocked(fullPath))
                    {
                        skippedCount++;
                        skippedFiles.Add(item.FilePath);
                        continue;
                    }

                    try
                    {
                        _repository.Index.Add(item.FilePath);
                        stagedCount++;
                    }
                    catch (Exception)
                    {
                        skippedCount++;
                        skippedFiles.Add(item.FilePath);
                    }
                }

                _repository.Index.Write();

                var message = $"已暫存 {stagedCount} 個檔案";
                if (skippedCount > 0)
                {
                    message += $"，跳過 {skippedCount} 個檔案（檔案使用中或錯誤）";
                }

                return new StageResult
                {
                    Success = true,
                    Message = message,
                    StagedCount = stagedCount,
                    SkippedCount = skippedCount,
                    SkippedFiles = skippedFiles
                };
            }
            catch (Exception ex)
            {
                return new StageResult
                {
                    Success = false,
                    Message = $"暫存變更時發生錯誤: {ex.Message}"
                };
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void Commit(string message, string authorName, string authorEmail)
        {
            if (_repository == null)
                throw new Exception("Git儲存庫未初始化");

            _repositoryLock.Wait();
            try
            {
                var statusInfo = GetDetailedStatus();
                if (!statusInfo.CanCommit)
                {
                    throw new Exception(statusInfo.Message);
                }

                var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
                var committer = author;
                _repository.Commit(message, author, committer);
            }
            catch (EmptyCommitException)
            {
                throw new Exception("無法建立空提交。請確認有檔案變更並且已經暫存。");
            }
            catch (UnbornBranchException)
            {
                throw new Exception("這是第一次提交。如果沒有檔案可提交，請先新增一些檔案到儲存庫中。");
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public RepositoryStatusInfo GetDetailedStatus()
        {
            if (_repository == null)
                return new RepositoryStatusInfo { IsValid = false, Message = "Git儲存庫未初始化" };

            _repositoryLock.Wait();
            try
            {
                var status = _repository.RetrieveStatus();

                var stagedFiles = status.Where(s => s.State.HasFlag(FileStatus.NewInIndex) ||
                                               s.State.HasFlag(FileStatus.ModifiedInIndex) ||
                                               s.State.HasFlag(FileStatus.DeletedFromIndex) ||
                                               s.State.HasFlag(FileStatus.RenamedInIndex) ||
                                               s.State.HasFlag(FileStatus.TypeChangeInIndex)).ToList();

                var unstagedFiles = status.Where(s => s.State.HasFlag(FileStatus.NewInWorkdir) ||
                                                 s.State.HasFlag(FileStatus.ModifiedInWorkdir) ||
                                                 s.State.HasFlag(FileStatus.DeletedFromWorkdir) ||
                                                 s.State.HasFlag(FileStatus.TypeChangeInWorkdir)).ToList();

                var untrackedFiles = status.Where(s => s.State.HasFlag(FileStatus.NewInWorkdir) &&
                                                  !s.State.HasFlag(FileStatus.NewInIndex)).ToList();

                return new RepositoryStatusInfo
                {
                    IsValid = true,
                    StagedFilesCount = stagedFiles.Count,
                    UnstagedFilesCount = unstagedFiles.Count,
                    UntrackedFilesCount = untrackedFiles.Count,
                    StagedFiles = stagedFiles.Select(f => f.FilePath).ToList(),
                    UnstagedFiles = unstagedFiles.Select(f => f.FilePath).ToList(),
                    UntrackedFiles = untrackedFiles.Select(f => f.FilePath).ToList(),
                    CanCommit = stagedFiles.Any(),
                    Message = stagedFiles.Any() ?
                        $"準備提交 {stagedFiles.Count} 個檔案" :
                        unstagedFiles.Any() ?
                            $"有 {unstagedFiles.Count} 個檔案變更未暫存" :
                            "工作目錄乾淨，沒有變更"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryStatusInfo
                {
                    IsValid = false,
                    Message = $"檢查儲存庫狀態失敗: {ex.Message}"
                };
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public IEnumerable<CommitInfo> GetCommitHistory(int maxCount = 100)
        {
            if (_repository == null) return Enumerable.Empty<CommitInfo>();
            _repositoryLock.Wait();
            try
            {
                return _repository.Commits
                    .Take(maxCount)
                    .Select(c => new CommitInfo
                    {
                        Sha = c.Sha,
                        ShortSha = c.Sha.Substring(0, Math.Min(7, c.Sha.Length)),
                        Message = c.Message,
                        Author = c.Author.Name,
                        Date = c.Author.When,
                        CommitDate = c.Committer.When
                    })
                    .ToList();
            }
            catch
            {
                return Enumerable.Empty<CommitInfo>();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void CreateBranch(string branchName)
        {
            if (_repository == null) return;
            _repositoryLock.Wait();
            try
            {
                _repository.CreateBranch(branchName);
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void CheckoutBranch(string branchName)
        {
            if (_repository == null) return;
            _repositoryLock.Wait();
            try
            {
                var branch = _repository.Branches[branchName];
                if (branch != null)
                {
                    Commands.Checkout(_repository, branch);
                }
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public IEnumerable<string> GetBranches()
        {
            if (_repository == null) return Enumerable.Empty<string>();
            _repositoryLock.Wait();
            try
            {
                return _repository.Branches.Select(b => b.FriendlyName).ToList();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public string GetCurrentBranch()
        {
            if (_repository == null) return string.Empty;
            _repositoryLock.Wait();
            try
            {
                return _repository.Head.FriendlyName;
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public IEnumerable<string> GetChangedFilesBetweenCommits(string fromCommitSha, string toCommitSha)
        {
            if (_repository == null) return Enumerable.Empty<string>();
            _repositoryLock.Wait();
            try
            {
                var fromCommit = _repository.Lookup<Commit>(fromCommitSha);
                var toCommit = _repository.Lookup<Commit>(toCommitSha);
                if (fromCommit == null || toCommit == null) return Enumerable.Empty<string>();

                var changes = _repository.Diff.Compare<TreeChanges>(fromCommit.Tree, toCommit.Tree);
                return changes.Select(c => c.Path).Where(path =>
                    path.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public byte[]? GetFileContentFromCommit(string commitSha, string filePath)
        {
            if (_repository == null) return null;
            _repositoryLock.Wait();
            try
            {
                var commit = _repository.Lookup<Commit>(commitSha);
                if (commit == null) return null;

                var blob = commit[filePath]?.Target as Blob;
                if (blob == null) return null;

                using var stream = blob.GetContentStream();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public bool IsIgnoringCadFiles()
        {
            if (string.IsNullOrEmpty(_repositoryPath)) return false;
            var gitignorePath = Path.Combine(_repositoryPath, ".gitignore");
            if (!File.Exists(gitignorePath)) return false;
            try
            {
                var content = File.ReadAllText(gitignorePath);
                return content.Contains("*.dwg") || content.Contains("*.dxf");
            }
            catch
            {
                return false;
            }
        }

        public void RevertToCommit(string commitSha, string authorName, string authorEmail)
        {
            if (_repository == null)
                throw new Exception("Git儲存庫未初始化");

            _repositoryLock.Wait();
            try
            {
                var commit = _repository.Lookup<Commit>(commitSha);
                if (commit == null)
                    throw new Exception($"找不到提交 {commitSha}");

                var status = _repository.RetrieveStatus();
                if (status.IsDirty)
                {
                    throw new Exception("工作目錄有未提交的變更，請先提交或暫存所有變更後再進行版本回復");
                }

                Commands.Checkout(_repository, commit, new CheckoutOptions
                {
                    CheckoutModifiers = CheckoutModifiers.Force
                });

                var statusAfterCheckout = _repository.RetrieveStatus();
                if (statusAfterCheckout.IsDirty)
                {
                    foreach (var item in statusAfterCheckout.Where(s =>
                        s.State.HasFlag(FileStatus.NewInWorkdir) ||
                        s.State.HasFlag(FileStatus.ModifiedInWorkdir) ||
                        s.State.HasFlag(FileStatus.DeletedFromWorkdir)))
                    {
                        if (item.State.HasFlag(FileStatus.DeletedFromWorkdir))
                        {
                            _repository.Index.Remove(item.FilePath);
                        }
                        else
                        {
                            _repository.Index.Add(item.FilePath);
                        }
                    }

                    _repository.Index.Write();

                    var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
                    var committer = author;
                    var message = $"回復到版本 {commitSha.Substring(0, 7)}";

                    _repository.Commit(message, author, committer);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"回復到版本 {commitSha} 失敗: {ex.Message}", ex);
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void ResetToCommit(string commitSha)
        {
            if (_repository == null)
                throw new Exception("Git儲存庫未初始化");

            _repositoryLock.Wait();
            try
            {
                var commit = _repository.Lookup<Commit>(commitSha);
                if (commit == null)
                    throw new Exception($"找不到提交 {commitSha}");

                _repository.Reset(ResetMode.Hard, commit);
            }
            catch (Exception ex)
            {
                throw new Exception($"重置到版本 {commitSha} 失敗: {ex.Message}", ex);
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void MergeBranch(string branchName)
        {
            if (_repository == null)
                throw new Exception("Git儲存庫未初始化");

            _repositoryLock.Wait();
            try
            {
                var branch = _repository.Branches[branchName];
                if (branch == null)
                    throw new Exception($"找不到分支 '{branchName}'");

                var currentBranch = _repository.Head;
                if (currentBranch.FriendlyName == branchName)
                    throw new Exception("無法合併分支到自己");

                var mergeResult = _repository.Merge(branch, new Signature("System", "system@gitdwg.com", DateTimeOffset.Now));
                
                if (mergeResult.Status == MergeStatus.Conflicts)
                {
                    throw new Exception("合併時發生衝突，請手動解決衝突後再試");
                }
                else if (mergeResult.Status == MergeStatus.UpToDate)
                {
                    throw new Exception("目標分支已經是最新版本，無需合併");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"合併分支 '{branchName}' 失敗: {ex.Message}", ex);
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void DeleteBranch(string branchName)
        {
            if (_repository == null)
                throw new Exception("Git儲存庫未初始化");

            _repositoryLock.Wait();
            try
            {
                var branch = _repository.Branches[branchName];
                if (branch == null)
                    throw new Exception($"找不到分支 '{branchName}'");

                var currentBranch = _repository.Head;
                if (currentBranch.FriendlyName == branchName)
                    throw new Exception("無法刪除當前分支");

                if (!_repository.Branches.Any(b => b.FriendlyName == "main" || b.FriendlyName == "master"))
                {
                    throw new Exception("無法確定主分支，請確保有 main 或 master 分支");
                }

                _repository.Branches.Remove(branch);
            }
            catch (Exception ex)
            {
                throw new Exception($"刪除分支 '{branchName}' 失敗: {ex.Message}", ex);
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        public void RenameBranch(string oldName, string newName)
        {
            if (_repository == null)
                throw new Exception("Git儲存庫未初始化");

            _repositoryLock.Wait();
            try
            {
                var branch = _repository.Branches[oldName];
                if (branch == null)
                    throw new Exception($"找不到分支 '{oldName}'");

                if (_repository.Branches[newName] != null)
                    throw new Exception($"分支名稱 '{newName}' 已經存在");

                if (!IsValidBranchName(newName))
                    throw new Exception("分支名稱包含無效字符，請使用字母、數字、連字符和底線");

                _repository.Branches.Rename(branch, newName);
            }
            catch (Exception ex)
            {
                throw new Exception($"重命名分支失敗: {ex.Message}", ex);
            }
            finally
            {
                _repositoryLock.Release();
            }
        }

        private bool IsValidBranchName(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
                return false;

            var invalidChars = new[] { ' ', '~', '^', ':', '?', '*', '[', '\\', '\t', '\n', '\r' };
            return !branchName.Any(c => invalidChars.Contains(c)) && 
                   !branchName.StartsWith("-") && 
                   !branchName.EndsWith(".") &&
                   !branchName.Contains("..");
        }

        private void DisposeRepository()
        {
            _repository?.Dispose();
            _repository = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _repositoryLock.Wait();
            try
            {
                DisposeRepository();
                _disposed = true;
            }
            finally
            {
                _repositoryLock.Release();
                _repositoryLock.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }

    public class CadFileStatus
    {
        public string FilePath { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsLocked { get; set; }
    }

    public class RepositoryStatusInfo
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StagedFilesCount { get; set; }
        public int UnstagedFilesCount { get; set; }
        public int UntrackedFilesCount { get; set; }
        public List<string> StagedFiles { get; set; } = new List<string>();
        public List<string> UnstagedFiles { get; set; } = new List<string>();
        public List<string> UntrackedFiles { get; set; } = new List<string>();
        public bool CanCommit { get; set; }
    }

    public class StageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StagedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> SkippedFiles { get; set; } = new List<string>();
    }
}