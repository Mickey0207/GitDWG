using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GitDWG.Services
{
    public class AutoCadCompareService
    {
        private string? _autocadExecutablePath;

        public AutoCadCompareService()
        {
            // 嘗試找到AutoCAD的安裝路徑
            FindAutoCADPath();
        }

        private void FindAutoCADPath()
        {
            // 常見的AutoCAD安裝路徑
            var possiblePaths = new[]
            {
                @"C:\Program Files\Autodesk\AutoCAD 2024\acad.exe",
                @"C:\Program Files\Autodesk\AutoCAD 2023\acad.exe",
                @"C:\Program Files\Autodesk\AutoCAD 2022\acad.exe",
                @"C:\Program Files\Autodesk\AutoCAD 2021\acad.exe",
                @"C:\Program Files (x86)\Autodesk\AutoCAD 2024\acad.exe",
                @"C:\Program Files (x86)\Autodesk\AutoCAD 2023\acad.exe",
                @"C:\Program Files (x86)\Autodesk\AutoCAD 2022\acad.exe",
                @"C:\Program Files (x86)\Autodesk\AutoCAD 2021\acad.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _autocadExecutablePath = path;
                    break;
                }
            }
        }

        public void SetAutoCADPath(string path)
        {
            if (File.Exists(path))
            {
                _autocadExecutablePath = path;
            }
        }

        public bool IsAutoCADAvailable()
        {
            return !string.IsNullOrEmpty(_autocadExecutablePath) && File.Exists(_autocadExecutablePath);
        }

        public async Task<bool> CompareDrawingsAsync(string file1Path, string file2Path)
        {
            if (!IsAutoCADAvailable())
            {
                throw new InvalidOperationException("AutoCAD not found. Please set the AutoCAD executable path.");
            }

            if (!File.Exists(file1Path) || !File.Exists(file2Path))
            {
                throw new FileNotFoundException("One or both drawing files do not exist.");
            }

            try
            {
                // 使用AutoCAD的比較功能
                // 這裡使用LISP腳本來執行比較
                var scriptPath = await CreateComparisonScript(file1Path, file2Path);
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = _autocadExecutablePath!,
                    Arguments = $"/b \"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        return process.ExitCode == 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error comparing drawings: {ex.Message}", ex);
            }
        }

        private async Task<string> CreateComparisonScript(string file1Path, string file2Path)
        {
            var tempPath = Path.GetTempPath();
            var scriptPath = Path.Combine(tempPath, "compare_drawings.scr");

            var script = $@"
(defun c:compare-drawings ()
  (command ""_DWGCOMPARE"" ""{file1Path}"" ""{file2Path}"" ""Y"")
  (princ)
)
(c:compare-drawings)
(command ""_QUIT"")
";

            await File.WriteAllTextAsync(scriptPath, script);
            return scriptPath;
        }

        public async Task<bool> OpenDrawingAsync(string filePath)
        {
            if (!IsAutoCADAvailable())
            {
                throw new InvalidOperationException("AutoCAD not found. Please set the AutoCAD executable path.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Drawing file does not exist.");
            }

            try
            {
                // 直接使用AutoCAD執行檔開啟，避免系統預設程式關聯問題
                var startInfo = new ProcessStartInfo
                {
                    FileName = _autocadExecutablePath!,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false, // 改為false以避免系統預設程式干擾
                    CreateNoWindow = false,
                    WorkingDirectory = Path.GetDirectoryName(_autocadExecutablePath)
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    // 等待一段時間確保AutoCAD啟動
                    await Task.Delay(1000);
                    return !process.HasExited;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error opening drawing with AutoCAD: {ex.Message}", ex);
            }
        }

        public async Task<bool> OpenDrawingsForComparisonAsync(string file1Path, string file2Path)
        {
            if (!IsAutoCADAvailable())
            {
                throw new InvalidOperationException("AutoCAD not found. Please set the AutoCAD executable path.");
            }

            if (!File.Exists(file1Path) || !File.Exists(file2Path))
            {
                throw new FileNotFoundException("One or both drawing files do not exist.");
            }

            try
            {
                // 先開啟第一個檔案
                var result1 = await OpenDrawingAsync(file1Path);
                if (!result1) return false;

                // 等待一段時間讓AutoCAD完全載入第一個檔案
                await Task.Delay(3000);

                // 再開啟第二個檔案（會在新的AutoCAD實例或新視窗中開啟）
                var result2 = await OpenDrawingAsync(file2Path);
                
                // 如果兩個檔案都成功開啟，嘗試使用DWG Compare功能
                if (result1 && result2)
                {
                    await Task.Delay(2000); // 等待第二個檔案載入
                    
                    // 創建並執行比較腳本
                    try
                    {
                        var scriptPath = await CreateAdvancedComparisonScript(file1Path, file2Path);
                        await ExecuteAutoCADScript(scriptPath);
                    }
                    catch (Exception ex)
                    {
                        // 即使比較腳本失敗，兩個檔案已經開啟，用戶可以手動比較
                        System.Diagnostics.Debug.WriteLine($"比較腳本執行失敗，但檔案已開啟: {ex.Message}");
                    }
                }

                return result1 && result2;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error opening drawings for comparison: {ex.Message}", ex);
            }
        }

        private async Task<string> CreateAdvancedComparisonScript(string file1Path, string file2Path)
        {
            var tempPath = Path.GetTempPath();
            var scriptPath = Path.Combine(tempPath, $"dwg_compare_{DateTime.Now:yyyyMMddHHmmss}.scr");

            // 建立更完整的比較腳本
            var script = $@"
; AutoCAD比較腳本
; 自動啟動DWG Compare功能
(defun c:auto-compare ()
  (command ""_.DWGCOMPARE"" ""{file1Path}"" ""{file2Path}"" """")
  (princ ""\nDWG比較已啟動"")
  (princ)
)
(c:auto-compare)
";

            await File.WriteAllTextAsync(scriptPath, script);
            return scriptPath;
        }

        private async Task ExecuteAutoCADScript(string scriptPath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _autocadExecutablePath!,
                    Arguments = $"/b \"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_autocadExecutablePath)
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing AutoCAD script: {ex.Message}", ex);
            }
        }

        public string GetAutoCADPath()
        {
            return _autocadExecutablePath ?? string.Empty;
        }
    }
}