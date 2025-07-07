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
            // ���է��AutoCAD���w�˸��|
            FindAutoCADPath();
        }

        private void FindAutoCADPath()
        {
            // �`����AutoCAD�w�˸��|
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
                // �ϥ�AutoCAD������\��
                // �o�̨ϥ�LISP�}���Ӱ�����
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
                // �����ϥ�AutoCAD�����ɶ}�ҡA�קK�t�ιw�]�{�����p���D
                var startInfo = new ProcessStartInfo
                {
                    FileName = _autocadExecutablePath!,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false, // �אּfalse�H�קK�t�ιw�]�{���z�Z
                    CreateNoWindow = false,
                    WorkingDirectory = Path.GetDirectoryName(_autocadExecutablePath)
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    // ���ݤ@�q�ɶ��T�OAutoCAD�Ұ�
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
                // ���}�ҲĤ@���ɮ�
                var result1 = await OpenDrawingAsync(file1Path);
                if (!result1) return false;

                // ���ݤ@�q�ɶ���AutoCAD�������J�Ĥ@���ɮ�
                await Task.Delay(3000);

                // �A�}�ҲĤG���ɮס]�|�b�s��AutoCAD��ҩηs�������}�ҡ^
                var result2 = await OpenDrawingAsync(file2Path);
                
                // �p�G����ɮ׳����\�}�ҡA���ըϥ�DWG Compare�\��
                if (result1 && result2)
                {
                    await Task.Delay(2000); // ���ݲĤG���ɮ׸��J
                    
                    // �Ыبð������}��
                    try
                    {
                        var scriptPath = await CreateAdvancedComparisonScript(file1Path, file2Path);
                        await ExecuteAutoCADScript(scriptPath);
                    }
                    catch (Exception ex)
                    {
                        // �Y�Ϥ���}�����ѡA����ɮפw�g�}�ҡA�Τ�i�H��ʤ��
                        System.Diagnostics.Debug.WriteLine($"����}�����楢�ѡA���ɮפw�}��: {ex.Message}");
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

            // �إߧ󧹾㪺����}��
            var script = $@"
; AutoCAD����}��
; �۰ʱҰ�DWG Compare�\��
(defun c:auto-compare ()
  (command ""_.DWGCOMPARE"" ""{file1Path}"" ""{file2Path}"" """")
  (princ ""\nDWG����w�Ұ�"")
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