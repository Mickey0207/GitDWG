# GitDWG 檔案操作錯誤解決指南

## ?? 常見檔案操作錯誤及解決方案

### 錯誤：Cannot create a file when that file already exists

#### ?? 錯誤描述
這個錯誤通常發生在保存用戶設定時，系統嘗試移動臨時檔案到目標位置，但目標檔案已經存在。

#### ?? 錯誤原因
1. **檔案鎖定**：檔案被其他程序占用
2. **權限問題**：沒有足夠的檔案系統權限
3. **並發操作**：多個操作同時嘗試修改同一檔案
4. **檔案系統問題**：磁碟錯誤或檔案系統損壞

#### ? 解決方案

##### 立即解決方法：
1. **重新啟動 GitDWG**
   - 完全關閉應用程式
   - 等待 5-10 秒
   - 重新開啟應用程式

2. **檢查檔案權限**
   ```
   檔案位置：%LOCALAPPDATA%\GitDWG\user_settings.json
   
   檢查步驟：
   - 開啟檔案總管
   - 導航到上述路徑
   - 右鍵點擊檔案 → 內容 → 安全性
   - 確認您的用戶帳戶有「完全控制」權限
   ```

3. **以管理員身份執行**
   - 右鍵點擊 GitDWG 捷徑
   - 選擇「以系統管理員身分執行」

4. **清理臨時檔案**
   ```powershell
   # 在 PowerShell 中執行
   Remove-Item "$env:LOCALAPPDATA\GitDWG\*.tmp" -Force -ErrorAction SilentlyContinue
   Remove-Item "$env:LOCALAPPDATA\GitDWG\*.backup" -Force -ErrorAction SilentlyContinue
   ```

##### 進階解決方法：

5. **重置用戶設定**
   - 在 GitDWG 中點擊「診斷提交問題」
   - 查看用戶設定檔案狀態
   - 如果有問題，可以手動刪除設定檔案重新開始

6. **檢查磁碟空間**
   - 確保系統碟有足夠空間（至少 1GB）
   - 清理暫存檔案和回收筒

7. **修復檔案系統**
   ```cmd
   # 在命令提示字元（管理員）中執行
   chkdsk C: /f /r
   ```

### 錯誤：UnauthorizedAccessException

#### ?? 錯誤描述
沒有權限存取設定檔案或目錄。

#### ? 解決方案

1. **檢查防毒軟體**
   - 將 GitDWG 安裝目錄加入防毒軟體白名單
   - 將 %LOCALAPPDATA%\GitDWG 加入白名單

2. **修復檔案權限**
   ```powershell
   # 在 PowerShell（管理員）中執行
   $path = "$env:LOCALAPPDATA\GitDWG"
   if (Test-Path $path) {
       icacls $path /grant:r "$env:USERNAME:(OI)(CI)F" /T
   }
   ```

3. **重新建立設定目錄**
   ```powershell
   # 備份現有設定（如果存在）
   $settingsPath = "$env:LOCALAPPDATA\GitDWG"
   if (Test-Path $settingsPath) {
       $backupPath = "$settingsPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
       Move-Item $settingsPath $backupPath
   }
   
   # 重新建立目錄
   New-Item -ItemType Directory -Path $settingsPath -Force
   ```

### 錯誤：IOException - 檔案被其他程序使用

#### ?? 錯誤描述
嘗試操作的檔案正被其他程序占用。

#### ? 解決方案

1. **檢查並關閉相關程序**
   ```powershell
   # 查找占用檔案的程序
   Get-Process | Where-Object { $_.MainWindowTitle -like "*GitDWG*" }
   ```

2. **使用工具檢查檔案占用**
   - 下載並使用 Process Explorer
   - 或使用 Resource Monitor（資源監視器）

3. **重新啟動電腦**
   - 如果無法找到占用程序，重新啟動電腦

## ?? 預防措施

### 定期維護
1. **定期清理臨時檔案**
   ```powershell
   # 建議每週執行一次
   $gitdwgPath = "$env:LOCALAPPDATA\GitDWG"
   Remove-Item "$gitdwgPath\*.tmp" -Force -ErrorAction SilentlyContinue
   Remove-Item "$gitdwgPath\*.backup" -Force -ErrorAction SilentlyContinue
   Remove-Item "$gitdwgPath\*.corrupted_*" -Force -ErrorAction SilentlyContinue
   ```

2. **備份重要設定**
   ```powershell
   # 備份用戶設定
   $source = "$env:LOCALAPPDATA\GitDWG\user_settings.json"
   $backup = "$env:USERPROFILE\Documents\GitDWG_settings_backup_$(Get-Date -Format 'yyyyMMdd').json"
   if (Test-Path $source) {
       Copy-Item $source $backup
   }
   ```

### 最佳實踐
1. **不要同時運行多個 GitDWG 實例**
2. **定期重新啟動應用程式**
3. **保持系統更新**
4. **確保防毒軟體不會干擾應用程式**

## ?? 診斷工具

### 使用 GitDWG 內建診斷
1. 開啟 GitDWG
2. 點擊「工具」→「診斷提交問題」
3. 查看用戶設定檔案狀態
4. 根據診斷結果採取相應措施

### 手動檢查清單
```powershell
# 完整診斷腳本
$gitdwgPath = "$env:LOCALAPPDATA\GitDWG"
$settingsFile = "$gitdwgPath\user_settings.json"

Write-Host "=== GitDWG 診斷報告 ===" -ForegroundColor Green
Write-Host "時間: $(Get-Date)" -ForegroundColor Gray
Write-Host ""

# 檢查目錄
Write-Host "檢查目錄..." -ForegroundColor Yellow
if (Test-Path $gitdwgPath) {
    Write-Host "? GitDWG 目錄存在: $gitdwgPath" -ForegroundColor Green
    
    # 檢查權限
    try {
        $testFile = "$gitdwgPath\test_$(Get-Random).tmp"
        "test" | Out-File $testFile -Encoding UTF8
        Remove-Item $testFile -Force
        Write-Host "? 目錄具有寫入權限" -ForegroundColor Green
    } catch {
        Write-Host "? 目錄沒有寫入權限: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "? GitDWG 目錄不存在" -ForegroundColor Red
}

# 檢查設定檔案
Write-Host "`n檢查設定檔案..." -ForegroundColor Yellow
if (Test-Path $settingsFile) {
    $fileInfo = Get-Item $settingsFile
    Write-Host "? 設定檔案存在" -ForegroundColor Green
    Write-Host "   大小: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "   最後修改: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    Write-Host "   唯讀: $($fileInfo.IsReadOnly)" -ForegroundColor Gray
} else {
    Write-Host "?? 設定檔案不存在" -ForegroundColor Yellow
}

# 檢查臨時檔案
Write-Host "`n檢查臨時檔案..." -ForegroundColor Yellow
$tempFiles = Get-ChildItem "$gitdwgPath\*.tmp" -ErrorAction SilentlyContinue
if ($tempFiles) {
    Write-Host "?? 發現 $($tempFiles.Count) 個臨時檔案" -ForegroundColor Yellow
    $tempFiles | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Gray }
} else {
    Write-Host "? 沒有臨時檔案" -ForegroundColor Green
}

# 檢查磁碟空間
Write-Host "`n檢查磁碟空間..." -ForegroundColor Yellow
$drive = Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DeviceID -eq "C:" }
$freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
if ($freeSpaceGB -gt 1) {
    Write-Host "? 磁碟空間充足: $freeSpaceGB GB" -ForegroundColor Green
} else {
    Write-Host "?? 磁碟空間不足: $freeSpaceGB GB" -ForegroundColor Yellow
}

Write-Host "`n=== 診斷完成 ===" -ForegroundColor Green
```

## ?? 技術支援

如果上述方法都無法解決問題，請聯絡技術支援並提供：

1. **錯誤截圖**
2. **診斷報告結果**
3. **Windows 版本資訊**
4. **GitDWG 版本資訊**
5. **最近的操作步驟**

---

*最後更新：2024年* | *適用版本：GitDWG v1.3.0+*