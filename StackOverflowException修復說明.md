# GitDWG StackOverflowException �״_����

## ���D�y�z
���I�睊�����v�ɡA�X�{�F `System.StackOverflowException` ���`�A�o�O�ѩ��ݩ��ܧ�q�����L�����j�եξɭP���C

## ���D�ڦ]
�b `MainViewModel` ���A�H�U�`���ե���ɭP�F�L�����j�G

1. `SelectedCommit` �ݩʳQ�]�m
2. Ĳ�o `((RelayCommand)CompareCommitsCommand).RaiseCanExecuteChanged()`
3. `RaiseCanExecuteChanged()` �ե� `CanCompareCommits()` ��k
4. `CanCompareCommits()` �X�� `SelectedCommit` �ݩ�
5. �b�Y�Ǳ��p�UĲ�o�F�ݩ��ܧ�A�Φ��L���`��

## �״_���I

### 1. �K�[�Ȥ�� (Value Comparison)
�b�Ҧ��ݩʪ� setter ���K�[�Ȥ���A�קK�����n���ݩ��ܧ�q���G

```csharp
public CommitInfo? SelectedCommit
{
    get => _selectedCommit;
    set
    {
        if (_selectedCommit != value)  // �Ȥ��
        {
            _selectedCommit = value;
            OnPropertyChanged();
            // ... ��L�޿�
        }
    }
}
```

### 2. ���`�O�@ (Exception Protection)
�b�Ҧ��i��ް_���j����k�եΤ��K�[ try-catch �O�@�G

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

### 3. �ϥΨp���r�q (Private Field Access)
�b `CanCompareCommits` ��k�������ϥΨp���r�q�A�קKĲ�o�ݩʳX�ݡG

```csharp
private bool CanCompareCommits()
{
    try
    {
        // �����ϥΨp���r�q�קKĲ�o�ݩʳX��
        return _selectedCommit != null && _isRepositoryLoaded;
    }
    catch
    {
        return false;
    }
}
```

## �w�״_���ݩ�
�H�U�ݩʤw�g�L�״_�A��ƨ����j�O�@�G

- ? `SelectedCommit`
- ? `CommitMessage`  
- ? `AuthorName`
- ? `AuthorEmail`
- ? `IsRepositoryLoaded`

## ��������
�״_��A�д��եH�U�ާ@�T�O���D�w�ѨM�G

1. **�I�睊�����v** - ���Ӥ��A�X�{ StackOverflowException
2. **��ܤ��P����** - ���ӯॿ�`��ܩM����
3. **��������\��** - ���ӯॿ�`�Ұʹϭ����
4. **UI�T����** - �ɭ����ӫO���T���A���|�ᵲ

## �w�����I
���F�קK�N�ӥX�{�������D�G

1. **�ݩʳ]�p��h**�G
   - �`�O�b setter ���i��Ȥ��
   - ��i��ް_�Ƨ@�Ϊ��եβK�[���`�O�@
   - �קK�b�ݩʤ���������޿�

2. **�R�O�]�p��h**�G
   - �b `CanExecute` ��k���ϥΨp���r�q�ӫD�ݩ�
   - �קK�b `CanExecute` ��Ĳ�o�����ݩ��ܧ�
   - �K�[�A�����`�B�z

3. **�ոէޥ�**�G
   - �ϥ� `System.Diagnostics.Debug.WriteLine` �O�����`
   - �b�}�o�L�{���ʱ��եΰ��|�`��
   - �w���ˬd�O�_����b���`���̿�

## �ʯ�v�T
�״_���I��ʯ઺�v�T�G

- ? **�����v�T**�G��֤����n���ݩ��ܧ�q��
- ? **�����v�T**�G�קK�L�����j�A����í�w��
- ? **���L�}�P**�G�W�[�F�Ȥ���M���`�B�z

�`��Ө��A�״_�᪺�N�X��[í�w�M���ġC

## ������@
1. �w���ˬd�s�K�[���ݩʬO�_��`�ۦP���Ҧ�
2. �b�K�[�s���R�O�ɽT�O `CanExecute` ��k���w����
3. �ʱ����ε{�������`��x�A�ήɵo�{��b���D

---

**�״_���A**: ? �w����
**���ժ��A**: ? �ظm���\
**�v�T�d��**: MainViewModel �ݩʩM�R�O�t��