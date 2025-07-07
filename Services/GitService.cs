using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitDWG.Services
{
    public class GitService
    {
        private string _repositoryPath;
        private Repository? _repository;

        public GitService(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
            
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
                var gitignoreContent = @"# AutoCAD�Ȧs�ɮ�
*.bak
*.dwl
*.dwl2
*.sv$
*.tmp
*.$$$
*.ac$
*.dwg.lock

# Windows�t���ɮ�
Thumbs.db
Desktop.ini
.DS_Store

# ��L�Ȧs�ɮ�
*.log
*.temp
";
                File.WriteAllText(gitignorePath, gitignoreContent);
            }

            var gitattributesPath = Path.Combine(_repositoryPath, ".gitattributes");
            if (!File.Exists(gitattributesPath))
            {
                var gitattributesContent = @"# CAD�ɮ׳]�w���G�i���ɮ�
*.dwg binary
*.dxf text
*.dwt binary
*.dws binary

# ��L�G�i��榡
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
            _repositoryPath = repositoryPath;
            _repository?.Dispose();
            _repository = null;
            
            if (!string.IsNullOrEmpty(_repositoryPath) && Repository.IsValid(_repositoryPath))
            {
                _repository = new Repository(_repositoryPath);
            }
        }

        public IEnumerable<string> GetStatusChanges()
        {
            if (_repository == null) return new List<string>();

            try
            {
                var status = _repository.RetrieveStatus();
                return status.Select(s => $"{s.State}: {s.FilePath}");
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public IEnumerable<CadFileStatus> GetCadFileChanges()
        {
            if (_repository == null) return new List<CadFileStatus>();

            try
            {
                var status = _repository.RetrieveStatus();
                var cadFiles = status.Where(s => 
                    s.FilePath.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) ||
                    s.FilePath.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase) ||
                    s.FilePath.EndsWith(".dwt", StringComparison.OrdinalIgnoreCase) ||
                    s.FilePath.EndsWith(".dws", StringComparison.OrdinalIgnoreCase));

                return cadFiles.Select(s => new CadFileStatus
                {
                    FilePath = s.FilePath,
                    State = s.State.ToString(),
                    FileSize = GetFileSize(Path.Combine(_repositoryPath, s.FilePath)),
                    LastModified = GetLastModified(Path.Combine(_repositoryPath, s.FilePath)),
                    IsLocked = IsFileLocked(Path.Combine(_repositoryPath, s.FilePath))
                });
            }
            catch (Exception)
            {
                return new List<CadFileStatus>();
            }
        }

        private bool IsFileLocked(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
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

            try
            {
                _repository.RetrieveStatus(new StatusOptions());
            }
            catch (Exception)
            {
                try
                {
                    _repository.Dispose();
                    _repository = new Repository(_repositoryPath);
                }
                catch
                {
                    _repository = null;
                }
            }
        }

        public StageResult StageAllChangesWithResult()
        {
            if (_repository == null) 
                return new StageResult { Success = false, Message = "Git�x�s�w����l��" };

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
                        Message = "�S���ɮ׻ݭn�Ȧs",
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

                var message = $"�w�Ȧs {stagedCount} ���ɮ�";
                if (skippedCount > 0)
                {
                    message += $"�A���L {skippedCount} ���ɮס]�ɮרϥΤ��ο��~�^";
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
                    Message = $"�Ȧs�ܧ�ɵo�Ϳ��~: {ex.Message}" 
                };
            }
        }

        public void Commit(string message, string authorName, string authorEmail)
        {
            if (_repository == null) 
                throw new Exception("Git�x�s�w����l��");

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
                throw new Exception("�L�k�إߪŴ���C�нT�{���ɮ��ܧ�åB�w�g�Ȧs�C");
            }
            catch (UnbornBranchException)
            {
                throw new Exception("�o�O�Ĥ@������C�p�G�S���ɮץi����A�Х��s�W�@���ɮר��x�s�w���C");
            }
        }

        public RepositoryStatusInfo GetDetailedStatus()
        {
            if (_repository == null) 
                return new RepositoryStatusInfo { IsValid = false, Message = "Git�x�s�w����l��" };

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
                        $"�ǳƴ��� {stagedFiles.Count} ���ɮ�" : 
                        unstagedFiles.Any() ? 
                            $"�� {unstagedFiles.Count} ���ɮ��ܧ󥼼Ȧs" : 
                            "�u�@�ؿ����b�A�S���ܧ�"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryStatusInfo 
                { 
                    IsValid = false, 
                    Message = $"�ˬd�x�s�w���A����: {ex.Message}" 
                };
            }
        }

        public IEnumerable<CommitInfo> GetCommitHistory(int maxCount = 100)
        {
            if (_repository == null) return new List<CommitInfo>();

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
                });
        }

        public void CreateBranch(string branchName)
        {
            if (_repository == null) return;
            _repository.CreateBranch(branchName);
        }

        public void CheckoutBranch(string branchName)
        {
            if (_repository == null) return;

            var branch = _repository.Branches[branchName];
            if (branch != null)
            {
                Commands.Checkout(_repository, branch);
            }
        }

        public IEnumerable<string> GetBranches()
        {
            if (_repository == null) return new List<string>();
            return _repository.Branches.Select(b => b.FriendlyName);
        }

        public string GetCurrentBranch()
        {
            if (_repository == null) return string.Empty;
            return _repository.Head.FriendlyName;
        }

        public IEnumerable<string> GetChangedFilesBetweenCommits(string fromCommitSha, string toCommitSha)
        {
            if (_repository == null) return new List<string>();

            var fromCommit = _repository.Lookup<Commit>(fromCommitSha);
            var toCommit = _repository.Lookup<Commit>(toCommitSha);

            if (fromCommit == null || toCommit == null) return new List<string>();

            var changes = _repository.Diff.Compare<TreeChanges>(fromCommit.Tree, toCommit.Tree);
            return changes.Select(c => c.Path).Where(path => 
                path.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) || 
                path.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase));
        }

        public byte[]? GetFileContentFromCommit(string commitSha, string filePath)
        {
            if (_repository == null) return null;

            try
            {
                var commit = _repository.Lookup<Commit>(commitSha);
                if (commit == null) return null;

                var blob = commit[filePath]?.Target as Blob;
                if (blob == null) return null;

                using (var stream = blob.GetContentStream())
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
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
                throw new Exception("Git�x�s�w����l��");

            try
            {
                var commit = _repository.Lookup<Commit>(commitSha);
                if (commit == null)
                    throw new Exception($"�䤣�촣�� {commitSha}");

                // �ˬd�O�_�������檺�ܧ�
                var status = _repository.RetrieveStatus();
                if (status.IsDirty)
                {
                    throw new Exception("�u�@�ؿ��������檺�ܧ�A�Х�����μȦs�Ҧ��ܧ��A�i�檩���^�_");
                }

                // �ϥ�checkout�N�u�@�ؿ����m����w���檺���A
                Commands.Checkout(_repository, commit, new CheckoutOptions
                {
                    CheckoutModifiers = CheckoutModifiers.Force
                });

                // �ˬd�O�_�ݭn�Ыطs����ӫO�s�o�Ӫ��A
                var statusAfterCheckout = _repository.RetrieveStatus();
                if (statusAfterCheckout.IsDirty)
                {
                    // �Ȧs�Ҧ��ܧ�
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

                    // �Ыطs����ӫO�s�^�_�����A
                    var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
                    var committer = author;
                    var message = $"�^�_�쪩�� {commitSha.Substring(0, 7)}";

                    _repository.Commit(message, author, committer);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"�^�_�쪩�� {commitSha} ����: {ex.Message}", ex);
            }
        }

        public void ResetToCommit(string commitSha)
        {
            if (_repository == null)
                throw new Exception("Git�x�s�w����l��");

            try
            {
                var commit = _repository.Lookup<Commit>(commitSha);
                if (commit == null)
                    throw new Exception($"�䤣�촣�� {commitSha}");

                // ����hard reset�ާ@
                _repository.Reset(ResetMode.Hard, commit);
            }
            catch (Exception ex)
            {
                throw new Exception($"���m�쪩�� {commitSha} ����: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _repository?.Dispose();
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