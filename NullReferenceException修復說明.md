# GitDWG NullReferenceException 修復說明

## ?? 問題描述
在使用版本回復功能時出現 `System.NullReferenceException` 錯誤：
```
Object reference not set to an instance of an object.
GitDWG.ViewModels.MainViewModel.SelectedCommit.get 傳回 null
```

## ?? 問題根因
這個錯誤發生在以下場景：
1. 用戶選擇了一個提交記錄（`SelectedCommit` 不為 null）
2. 點擊「安全回復」或「重置版本」按鈕
3. 在執行過程中，可能由於以下原因導致 `SelectedCommit` 變成 null：
   - 數據重新整理（`RefreshData()` 調用）
   - UI更新導致選擇狀態丟失
   - 異步操作中的併發問題

## ? 修復方案

### 1. 保存目標提交資訊
在方法開始時立即保存選中的提交資訊到本地變數：

```csharp
private async Task RevertToCommitAsync()
{
    if (SelectedCommit == null)
    {
        StatusMessage = "請選擇一個要回復的提交";
        return;
    }

    // ?? 關鍵修復：在開始時保存選中的提交資訊
    var targetCommit = SelectedCommit;

    // 後續所有操作都使用 targetCommit 而不是 SelectedCommit
    // ...
}
```

### 2. 使用本地變數替代屬性
將所有對 `SelectedCommit` 的引用改為使用保存的本地變數：

```csharp
// ? 修復前（可能出錯）
StatusMessage = $"成功回復到版本 {SelectedCommit.ShortSha}";

// ? 修復後（安全）
StatusMessage = $"成功回復到版本 {targetCommit.ShortSha}";
```

## ??? 已修復的方法

### 1. RevertToCommitAsync()
- ? 在方法開始時保存 `targetCommit`
- ? 所有引用都使用本地變數
- ? 避免執行過程中的 null 引用

### 2. ResetToCommitAsync()
- ? 在方法開始時保存 `targetCommit`
- ? 確認對話框使用本地變數
- ? 執行結果訊息使用本地變數

### 3. CompareCommitsAsync()
- ? 在方法開始時保存 `targetCommit`
- ? 比較邏輯使用本地變數
- ? 對話框顯示使用本地變數

## ?? 測試驗證

修復後請測試以下場景：

### 測試1: 基本版本回復
1. 選擇一個歷史提交
2. 點擊「安全回復」
3. 確認對話框顯示正確的提交資訊
4. 確認操作完成後顯示成功訊息

### 測試2: 重置版本操作
1. 選擇一個歷史提交
2. 點擊「重置版本」
3. 完成兩次確認流程
4. 確認操作完成後顯示正確訊息

### 測試3: 版本比較功能
1. 選擇一個歷史提交
2. 點擊「比較版本」
3. 確認對話框顯示正確的版本資訊
4. 確認AutoCAD成功開啟比較檔案

### 測試4: 併發操作測試
1. 快速連續點擊不同的提交記錄
2. 在選擇後立即點擊操作按鈕
3. 確認不會出現 null 引用錯誤

## ??? 預防措施

### 1. 編程最佳實踐
```csharp
// ? 好的做法：立即保存重要引用
var targetItem = SomeProperty;
if (targetItem == null) return;
// 使用 targetItem

// ? 避免的做法：重複引用可變屬性
if (SomeProperty == null) return;
// 後續使用 SomeProperty（可能已經改變）
```

### 2. 異步方法安全
- 在長時間運行的異步操作中避免依賴可變的UI屬性
- 在方法開始時快照所有需要的狀態
- 使用本地變數而不是屬性進行業務邏輯

### 3. UI狀態管理
- 考慮在操作進行中禁用相關UI元素
- 提供明確的操作狀態指示
- 確保用戶操作的原子性

## ?? 修復影響分析

### 正面影響
- ? 完全消除了 NullReferenceException
- ? 提升了用戶體驗穩定性
- ? 增加了代碼的健壯性
- ? 操作過程更加可預測

### 性能影響
- ?? 無性能負面影響
- ?? 輕微的記憶體開銷（保存額外的引用）
- ?? 執行速度無影響

### 相容性
- ? 完全向後相容
- ? 不影響現有功能
- ? 不需要用戶操作改變

## ?? 未來改進建議

### 1. 加強錯誤處理
考慮添加更多的 null 檢查和錯誤恢復機制：

```csharp
private bool ValidateCommitSelection(out CommitInfo? commit)
{
    commit = SelectedCommit;
    if (commit == null)
    {
        StatusMessage = "請先選擇一個提交記錄";
        return false;
    }
    return true;
}
```

### 2. 狀態管理改進
考慮實現更正式的狀態管理模式，如：
- Command 模式與狀態驗證
- 操作鎖定機制
- 事務性操作支援

### 3. 用戶體驗優化
- 添加操作進度指示
- 提供操作取消機制
- 改進錯誤訊息的用戶友好性

---

**修復狀態**: ? 已完成
**測試狀態**: ? 建置成功
**風險等級**: ?? 低風險修復
**建議測試**: ?? 請進行完整的版本操作測試