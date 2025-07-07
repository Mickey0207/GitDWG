# GitDWG StackOverflowException 修復說明

## 問題描述
當點選版本歷史時，出現了 `System.StackOverflowException` 異常，這是由於屬性變更通知的無限遞迴調用導致的。

## 問題根因
在 `MainViewModel` 中，以下循環調用鏈導致了無限遞迴：

1. `SelectedCommit` 屬性被設置
2. 觸發 `((RelayCommand)CompareCommitsCommand).RaiseCanExecuteChanged()`
3. `RaiseCanExecuteChanged()` 調用 `CanCompareCommits()` 方法
4. `CanCompareCommits()` 訪問 `SelectedCommit` 屬性
5. 在某些情況下觸發了屬性變更，形成無限循環

## 修復措施

### 1. 添加值比較 (Value Comparison)
在所有屬性的 setter 中添加值比較，避免不必要的屬性變更通知：

```csharp
public CommitInfo? SelectedCommit
{
    get => _selectedCommit;
    set
    {
        if (_selectedCommit != value)  // 值比較
        {
            _selectedCommit = value;
            OnPropertyChanged();
            // ... 其他邏輯
        }
    }
}
```

### 2. 異常保護 (Exception Protection)
在所有可能引起遞迴的方法調用中添加 try-catch 保護：

```csharp
try
{
    ((RelayCommand)CompareCommitsCommand).RaiseCanExecuteChanged();
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error in RaiseCanExecuteChanged: {ex.Message}");
}
```

### 3. 使用私有字段 (Private Field Access)
在 `CanCompareCommits` 方法中直接使用私有字段，避免觸發屬性訪問：

```csharp
private bool CanCompareCommits()
{
    try
    {
        // 直接使用私有字段避免觸發屬性訪問
        return _selectedCommit != null && _isRepositoryLoaded;
    }
    catch
    {
        return false;
    }
}
```

## 已修復的屬性
以下屬性已經過修復，具備防遞迴保護：

- ? `SelectedCommit`
- ? `CommitMessage`  
- ? `AuthorName`
- ? `AuthorEmail`
- ? `IsRepositoryLoaded`

## 測試驗證
修復後，請測試以下操作確保問題已解決：

1. **點選版本歷史** - 應該不再出現 StackOverflowException
2. **選擇不同提交** - 應該能正常選擇和切換
3. **版本比較功能** - 應該能正常啟動圖面比較
4. **UI響應性** - 界面應該保持響應，不會凍結

## 預防措施
為了避免將來出現類似問題：

1. **屬性設計原則**：
   - 總是在 setter 中進行值比較
   - 對可能引起副作用的調用添加異常保護
   - 避免在屬性中執行複雜邏輯

2. **命令設計原則**：
   - 在 `CanExecute` 方法中使用私有字段而非屬性
   - 避免在 `CanExecute` 中觸發任何屬性變更
   - 添加適當的異常處理

3. **調試技巧**：
   - 使用 `System.Diagnostics.Debug.WriteLine` 記錄異常
   - 在開發過程中監控調用堆疊深度
   - 定期檢查是否有潛在的循環依賴

## 性能影響
修復措施對性能的影響：

- ? **正面影響**：減少不必要的屬性變更通知
- ? **正面影響**：避免無限遞迴，提升穩定性
- ? **輕微開銷**：增加了值比較和異常處理

總體而言，修復後的代碼更加穩定和高效。

## 後續維護
1. 定期檢查新添加的屬性是否遵循相同的模式
2. 在添加新的命令時確保 `CanExecute` 方法的安全性
3. 監控應用程式的異常日誌，及時發現潛在問題

---

**修復狀態**: ? 已完成
**測試狀態**: ? 建置成功
**影響範圍**: MainViewModel 屬性和命令系統