# ?? GitDWG 部署問題完整診斷和解決指南

## ?? 當前診斷狀態

**專案狀態**: ? 建置成功  
**Package.appxmanifest**: ? 格式正確  
**Assets檔案**: ? 全部存在  
**開發人員模式**: ? 已啟用  
**現有套件**: ? 無衝突  

---

## ?? 部署問題分類診斷

### 1. **立即檢查 - Configuration Manager設定**

#### Visual Studio 設定檢查
1. 開啟 Visual Studio
2. 選擇選單：**建置** → **Configuration Manager**
3. 確認以下設定：

| 設定項目 | 建議值 | 檢查狀態 |
|----------|--------|----------|
| **專案名稱** | GitDWG | □ |
| **設定** | Debug (或 Release) | □ |
| **平台** | x64 (推薦) | □ |
| **建置** | ? 已勾選 | □ |
| **部署** | ? 已勾選 | ?? **關鍵** |

#### 如果"部署"未勾選
1. 在Configuration Manager中勾選GitDWG專案的"部署"核取方塊
2. 點擊"確定"
3. 重新嘗試F5偵錯

### 2. **常見部署錯誤類型和解決方案**

#### A. DEP0700 - 應用程式註冊失敗
**症狀**: 套件註冊失敗
**解決方案**:
```powershell
# 清理舊套件
Get-AppxPackage *GitDWG* | Remove-AppxPackage -ErrorAction SilentlyContinue

# 清理Visual Studio快取
# 刪除 bin 和 obj 資料夾
```

#### B. 0x80073CF6 - 套件已安裝且版本更高
**症狀**: 無法安裝較低版本
**解決方案**:
```xml
<!-- 在Package.appxmanifest中增加版本號 -->
<Identity Version="1.3.1.0" />
```

#### C. 0x80073D06 - 套件簽章問題
**症狀**: 簽章驗證失敗
**解決方案**:
1. 確保使用Debug設定進行偵錯
2. 檢查開發人員模式已啟用

#### D. 0x80073CF9 - 套件更新失敗
**症狀**: 無法更新現有套件
**解決方案**:
```powershell
# 完全移除現有套件
Get-AppxPackage *GitDWGDevelopmentTeam* | Remove-AppxPackage
```

### 3. **詳細部署檢查清單**

#### 步驟1: 環境檢查
```powershell
# 檢查Windows版本
Get-ComputerInfo | Select WindowsProductName, WindowsVersion

# 檢查.NET 8運行時
dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 8"

# 檢查開發人員模式
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name AllowDevelopmentWithoutDevLicense
```

#### 步驟2: 專案檢查
```
□ GitDWG.csproj 設定正確
□ Package.appxmanifest 格式有效
□ Assets 資料夾包含所有必要圖示
□ 目標框架為 net8.0-windows
□ 專案建置無錯誤
```

#### 步驟3: Visual Studio檢查
```
□ 啟動專案設定為 GitDWG
□ Configuration Manager 中"部署"已勾選
□ 平台設定正確 (x64推薦)
□ 無編譯警告或錯誤
```

### 4. **逐步部署測試**

#### 測試1: 基本建置
```
1. 清理解決方案 (Build → Clean Solution)
2. 重建解決方案 (Build → Rebuild Solution)
3. 檢查輸出視窗無錯誤
```

#### 測試2: 部署準備
```
1. 確認Configuration Manager設定
2. 檢查套件清單無衝突
3. 驗證所有資源檔案存在
```

#### 測試3: 實際部署
```
1. 按F5開始偵錯
2. 觀察輸出視窗部署訊息
3. 檢查應用程式是否正常啟動
```

---

## ??? 具體解決步驟

### 方案1: 標準部署修復
```
1. 關閉 Visual Studio
2. 刪除專案目錄下的 bin 和 obj 資料夾
3. 開啟 Visual Studio
4. 載入專案
5. 確認 Configuration Manager 設定
6. 重建解決方案
7. 按 F5 開始偵錯
```

### 方案2: 深度清理修復
```powershell
# PowerShell 執行 (以管理員身份)

# 1. 移除所有相關套件
Get-AppxPackage *GitDWG* | Remove-AppxPackage -ErrorAction SilentlyContinue

# 2. 清理套件快取
Remove-Item "$env:LOCALAPPDATA\Packages\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue

# 3. 清理暫存檔案
Remove-Item "$env:TEMP\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue
```

### 方案3: 開發人員模式重置
```
1. 開啟設定 → 更新與安全性 → 開發人員選項
2. 關閉開發人員模式
3. 重新啟動電腦
4. 重新啟用開發人員模式
5. 嘗試部署
```

### 方案4: 套件名稱檢查
確認Package.appxmanifest中的名稱格式正確：
```xml
<Identity
  Name="GitDWGDevelopmentTeam.GitDWG"    <!-- 正確格式 -->
  Publisher="CN=GitDWG Development Team"
  Version="1.3.0.0" />
```

---

## ?? 故障排除工具

### Visual Studio診斷
1. **輸出視窗**: 查看建置和部署訊息
2. **錯誤清單**: 檢查編譯錯誤和警告
3. **偵錯輸出**: 查看運行時訊息

### PowerShell診斷命令
```powershell
# 檢查套件狀態
Get-AppxPackage | Where-Object {$_.Name -like "*GitDWG*"}

# 檢查部署記錄
Get-WinEvent -LogName "Microsoft-Windows-AppxDeployment-Server/Operational" -MaxEvents 10

# 檢查開發人員設定
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
```

### 系統事件檢查
1. 開啟 **事件檢視器**
2. 導航到：**應用程式及服務記錄** → **Microsoft** → **Windows** → **AppxDeployment-Server**
3. 查看最新的錯誤或警告事件

---

## ?? 錯誤代碼對照表

| 錯誤代碼 | 說明 | 解決方案 |
|----------|------|----------|
| **DEP0700** | 應用程式註冊失敗 | 檢查套件清單、清理舊套件 |
| **0x80073CF6** | 版本衝突 | 增加版本號或移除舊版本 |
| **0x80073D06** | 簽章問題 | 檢查開發人員模式 |
| **0x80073CF9** | 更新失敗 | 完全移除後重新安裝 |
| **0x80073D02** | 套件損壞 | 重新建置專案 |
| **0x80070002** | 檔案未找到 | 檢查Assets檔案 |

---

## ?? 部署成功驗證清單

### 部署階段檢查
```
□ Visual Studio輸出顯示"部署成功"
□ 無錯誤或警告訊息
□ 套件已安裝到系統
□ 開始選單出現應用程式圖示
```

### 運行階段檢查
```
□ 應用程式正常啟動
□ 登入視窗正確顯示
□ 主要功能正常運作
□ 無運行時錯誤
```

### 功能驗證
```
□ 用戶登入系統
□ Git儲存庫操作
□ 分支圖形管理器
□ CAD檔案處理
□ 設定保存和載入
```

---

## ?? 快速修復命令集

### 一鍵清理腳本
```powershell
# 建立並執行此PowerShell腳本 (以管理員身份)
Write-Host "清理GitDWG部署環境..." -ForegroundColor Green

# 移除現有套件
Get-AppxPackage *GitDWG* | Remove-AppxPackage -ErrorAction SilentlyContinue
Write-Host "? 已清理現有套件" -ForegroundColor Green

# 清理快取
Remove-Item "$env:LOCALAPPDATA\Packages\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "? 已清理套件快取" -ForegroundColor Green

# 清理暫存檔案
Remove-Item "$env:TEMP\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "? 已清理暫存檔案" -ForegroundColor Green

Write-Host "清理完成！請重新啟動Visual Studio並嘗試部署。" -ForegroundColor Yellow
```

### Visual Studio重置步驟
```
1. 關閉所有Visual Studio實例
2. 執行上述清理腳本
3. 重新啟動電腦 (可選，但建議)
4. 開啟Visual Studio
5. 載入GitDWG專案
6. 檢查Configuration Manager設定
7. 按F5嘗試部署
```

---

## ?? 如果問題持續存在

### 收集診斷資訊
```
1. Visual Studio版本和組建號碼
2. Windows版本和組建號碼
3. 完整的錯誤訊息和堆疊追蹤
4. 事件檢視器中的相關錯誤
5. Package.appxmanifest的完整內容
```

### 進階診斷
```
1. 嘗試創建新的空白WinUI 3專案並部署
2. 檢查Visual Studio安裝是否完整
3. 確認Windows SDK版本相容性
4. 測試以管理員身份執行Visual Studio
```

---

## ?? 最後提醒

### 必須確認的關鍵點
1. ? **Configuration Manager中"部署"已勾選** - 這是最常見的問題
2. ? **開發人員模式已啟用**
3. ? **無現有套件衝突**
4. ? **Package.appxmanifest格式正確**
5. ? **所有Assets檔案存在**

### 推薦的部署順序
```
基本檢查 → 清理環境 → 重建專案 → 檢查設定 → 部署測試
```

**如果遵循此指南後仍無法部署，請提供具體的錯誤訊息以便進一步協助！** ??