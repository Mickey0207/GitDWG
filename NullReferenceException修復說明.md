# GitDWG NullReferenceException �״_����

## ?? ���D�y�z
�b�ϥΪ����^�_�\��ɥX�{ `System.NullReferenceException` ���~�G
```
Object reference not set to an instance of an object.
GitDWG.ViewModels.MainViewModel.SelectedCommit.get �Ǧ^ null
```

## ?? ���D�ڦ]
�o�ӿ��~�o�ͦb�H�U�����G
1. �Τ��ܤF�@�Ӵ���O���]`SelectedCommit` ���� null�^
2. �I���u�w���^�_�v�Ρu���m�����v���s
3. �b����L�{���A�i��ѩ�H�U��]�ɭP `SelectedCommit` �ܦ� null�G
   - �ƾڭ��s��z�]`RefreshData()` �եΡ^
   - UI��s�ɭP��ܪ��A�ᥢ
   - ���B�ާ@�����ֵo���D

## ? �״_���

### 1. �O�s�ؼд����T
�b��k�}�l�ɥߧY�O�s�襤�������T�쥻�a�ܼơG

```csharp
private async Task RevertToCommitAsync()
{
    if (SelectedCommit == null)
    {
        StatusMessage = "�п�ܤ@�ӭn�^�_������";
        return;
    }

    // ?? ����״_�G�b�}�l�ɫO�s�襤�������T
    var targetCommit = SelectedCommit;

    // ����Ҧ��ާ@���ϥ� targetCommit �Ӥ��O SelectedCommit
    // ...
}
```

### 2. �ϥΥ��a�ܼƴ��N�ݩ�
�N�Ҧ��� `SelectedCommit` ���ޥΧאּ�ϥΫO�s�����a�ܼơG

```csharp
// ? �״_�e�]�i��X���^
StatusMessage = $"���\�^�_�쪩�� {SelectedCommit.ShortSha}";

// ? �״_��]�w���^
StatusMessage = $"���\�^�_�쪩�� {targetCommit.ShortSha}";
```

## ??? �w�״_����k

### 1. RevertToCommitAsync()
- ? �b��k�}�l�ɫO�s `targetCommit`
- ? �Ҧ��ޥγ��ϥΥ��a�ܼ�
- ? �קK����L�{���� null �ޥ�

### 2. ResetToCommitAsync()
- ? �b��k�}�l�ɫO�s `targetCommit`
- ? �T�{��ܮبϥΥ��a�ܼ�
- ? ���浲�G�T���ϥΥ��a�ܼ�

### 3. CompareCommitsAsync()
- ? �b��k�}�l�ɫO�s `targetCommit`
- ? ����޿�ϥΥ��a�ܼ�
- ? ��ܮ���ܨϥΥ��a�ܼ�

## ?? ��������

�״_��д��եH�U�����G

### ����1: �򥻪����^�_
1. ��ܤ@�Ӿ��v����
2. �I���u�w���^�_�v
3. �T�{��ܮ���ܥ��T�������T
4. �T�{�ާ@��������ܦ��\�T��

### ����2: ���m�����ާ@
1. ��ܤ@�Ӿ��v����
2. �I���u���m�����v
3. �����⦸�T�{�y�{
4. �T�{�ާ@��������ܥ��T�T��

### ����3: ��������\��
1. ��ܤ@�Ӿ��v����
2. �I���u��������v
3. �T�{��ܮ���ܥ��T��������T
4. �T�{AutoCAD���\�}�Ҥ���ɮ�

### ����4: �ֵo�ާ@����
1. �ֳt�s���I�����P������O��
2. �b��ܫ�ߧY�I���ާ@���s
3. �T�{���|�X�{ null �ޥο��~

## ??? �w�����I

### 1. �s�{�̨ι��
```csharp
// ? �n�����k�G�ߧY�O�s���n�ޥ�
var targetItem = SomeProperty;
if (targetItem == null) return;
// �ϥ� targetItem

// ? �קK�����k�G���ƤޥΥi���ݩ�
if (SomeProperty == null) return;
// ����ϥ� SomeProperty�]�i��w�g���ܡ^
```

### 2. ���B��k�w��
- �b���ɶ��B�檺���B�ާ@���קK�̿�i�ܪ�UI�ݩ�
- �b��k�}�l�ɧַөҦ��ݭn�����A
- �ϥΥ��a�ܼƦӤ��O�ݩʶi��~���޿�

### 3. UI���A�޲z
- �Ҽ{�b�ާ@�i�椤�T�ά���UI����
- ���ѩ��T���ާ@���A����
- �T�O�Τ�ާ@����l��

## ?? �״_�v�T���R

### �����v�T
- ? ���������F NullReferenceException
- ? ���ɤF�Τ�����í�w��
- ? �W�[�F�N�X��������
- ? �ާ@�L�{��[�i�w��

### �ʯ�v�T
- ?? �L�ʯ�t���v�T
- ?? ���L���O����}�P�]�O�s�B�~���ޥΡ^
- ?? ����t�׵L�v�T

### �ۮe��
- ? �����V��ۮe
- ? ���v�T�{���\��
- ? ���ݭn�Τ�ާ@����

## ?? ���ӧ�i��ĳ

### 1. �[�j���~�B�z
�Ҽ{�K�[��h�� null �ˬd�M���~��_����G

```csharp
private bool ValidateCommitSelection(out CommitInfo? commit)
{
    commit = SelectedCommit;
    if (commit == null)
    {
        StatusMessage = "�Х���ܤ@�Ӵ���O��";
        return false;
    }
    return true;
}
```

### 2. ���A�޲z��i
�Ҽ{��{�󥿦������A�޲z�Ҧ��A�p�G
- Command �Ҧ��P���A����
- �ާ@��w����
- �ưȩʾާ@�䴩

### 3. �Τ������u��
- �K�[�ާ@�i�׫���
- ���Ѿާ@��������
- ��i���~�T�����Τ�ͦn��

---

**�״_���A**: ? �w����
**���ժ��A**: ? �ظm���\
**���I����**: ?? �C���I�״_
**��ĳ����**: ?? �жi�槹�㪺�����ާ@����