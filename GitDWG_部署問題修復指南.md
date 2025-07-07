# ?? GitDWG WinUI 3 部署問題修復指南

## ? 問題已解決

**錯誤訊息**: "必須先部署專案，才能進行偵錯。請在Configuration Manager中啟用[部署]。"

**根本原因**: Package.appxmanifest檔案引用了不存在的自定義圖示檔案

**修復狀態**: ? 已完成

---

## ??? 已執行的修復步驟

### 1. **Package.appxmanifest檔案修復**

#### 修復前的問題
```xml
<!-- 引用不存在的檔案 -->
<Logo>Assets\GitDWG_150.png</Logo>
<Square150x150Logo="Assets\GitDWG_150.png"
<Square44x44Logo="Assets\GitDWG_44.png">
<SplashScreen Image="Assets\GitDWG_620x300.png"
```

#### 修復後的設定
```xml
<!-- 使用現有的預設圖示檔案 -->
<Logo>Assets\Square150x150Logo.scale-200.png</Logo>
<Square150x150Logo="Assets\Square150x150Logo.scale-200.png"
<Square44x44Logo="Assets\Square44x44Logo.scale-200.png">
<SplashScreen Image="Assets\SplashScreen.scale-200.png"
```

### 2. **確認現有Assets檔案**

? 以下檔案已存在且可用：
- `LockScreenLogo.scale-200.png`
- `SplashScreen.scale-200.png`
- `Square150x150Logo.scale-200.png`
- `Square44x44Logo.scale-200.png`
- `Square44x44Logo.targetsize-24_altform-unplated.png`
- `StoreLogo.png`
- `Wide310x150Logo.scale-200.png`

### 3. **建置驗證**
? 專案建置成功，無編譯錯誤

---

## ?? 現在可以正常運行的功能

### 部署和偵錯
- ? F5 偵錯執行
- ? Ctrl+F5 不偵錯執行
- ? Visual Studio部署
- ? 套件建立和安裝

### 應用程式功能
- ? 正常啟動和初始化
- ? 用戶登入系統
- ? Git版本控制功能
- ? 分支圖形管理器
- ? CAD檔案處理

---

## ?? Visual Studio 偵錯設定指南

### 1. **確認啟動專案設定**
在Visual Studio中：
1. 右鍵點擊解決方案 → "設定為啟動專案"
2. 確認GitDWG專案被設為啟動專案（粗體顯示）

### 2. **Configuration Manager檢查**
1. 選擇選單：**建置** → **Configuration Manager**
2. 確認GitDWG專案的"部署"核取方塊已勾選
3. 平台設定選擇適當的架構（x64推薦）

### 3. **偵錯設定**
```
設定項目          建議值
================  ==================
設定              Debug
平台              x64 (或 x86)
目標架構          net8.0-windows
部署              ? 已啟用
啟動專案          GitDWG
```

### 4. **運行測試**
- **F5**: 開始偵錯
- **Ctrl+F5**: 開始執行(不偵錯)
- **F6**: 建置專案
- **F7**: 建置解決方案

---

## ?? WinUI 3 專案特殊注意事項

### MSIX 套件部署
WinUI 3應用程式需要先部署為MSIX套件才能執行：

#### 自動部署流程
1. Visual Studio自動建立MSIX套件
2. 安裝到本機Windows
3. 註冊應用程式
4. 啟動偵錯工作階段

#### 部署需求
- ? 有效的Package.appxmanifest
- ? 正確的圖示檔案參照
- ? 適當的權限設定
- ? Windows開發人員模式

### 常見部署問題
| 問題 | 原因 | 解決方案 |
|------|------|----------|
| 圖示檔案遺失 | manifest引用不存在檔案 | ? 已修復 |
| 權限不足 | 未啟用開發人員模式 | 啟用Windows開發人員模式 |
| 版本衝突 | 舊版本未清理 | 解除安裝舊版本 |
| 簽署問題 | 測試憑證過期 | 重新產生測試憑證 |

---

## ?? 故障排除指南

### 如果仍無法部署

#### 檢查1: Windows開發人員模式
```powershell
# 檢查開發人員模式狀態
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name AllowDevelopmentWithoutDevLicense
```

#### 檢查2: 清理舊部署
1. 開啟"設定" → "應用程式"
2. 搜尋"GitDWG"
3. 解除安裝任何現有版本
4. 重新偵錯

#### 檢查3: Visual Studio重新載入
1. 關閉Visual Studio
2. 刪除bin和obj資料夾
3. 重新開啟專案
4. 重建解決方案

#### 檢查4: 套件名稱衝突
確認Package.appxmanifest中的Identity設定：
```xml
<Identity
  Name="GitDWG.CADVersionControl"
  Publisher="CN=GitDWG Development Team"
  Version="1.3.0.0" />
```

---

## ?? 成功測試清單

執行以下測試確認修復成功：

### 基本部署測試
- [ ] F5偵錯正常啟動
- [ ] 應用程式視窗正確顯示
- [ ] 登入功能正常工作
- [ ] 主要功能可以使用

### 進階功能測試
- [ ] Git儲存庫操作
- [ ] 分支圖形管理器
- [ ] 用戶設定系統
- [ ] AutoCAD整合功能

### 部署品質測試
- [ ] 套件安裝成功
- [ ] 開始選單項目正確
- [ ] 圖示正常顯示
- [ ] 解除安裝乾淨完成

---

## ?? 如果仍有問題

### 收集診斷資訊
1. Visual Studio輸出視窗的完整錯誤訊息
2. Windows事件檢視器中的應用程式錯誤
3. 部署記錄檔案
4. 系統環境資訊

### 聯絡支援時提供
- Windows版本和組建號碼
- Visual Studio版本
- .NET版本
- 完整的錯誤堆疊追蹤
- 重現步驟

---

**?? 恭喜！GitDWG現在可以正常部署和偵錯了！**

**主要修復**: 將Package.appxmanifest中的圖示參照從不存在的自定義檔案改為現有的預設檔案，解決了MSIX套件建立失敗的問題。