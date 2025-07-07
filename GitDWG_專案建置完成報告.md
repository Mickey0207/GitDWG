# GitDWG v1.3.0 - ����M�׫ظm���\���i

## ?? �ظm���\���A

**? �M�׫ظm�G���\**  
**? �sĶ���~�G�w�����״_**  
**? �֤ߥ\��G�����{**  

---

## ? �w�������֤ߥ\��

### 1. ?? ���㪺Git����޲z�t��

#### Git�֤ߥ\��
- ? **�x�s�w�޲z**�G��l�ơB��ܡB����Git�x�s�w
- ? **����ާ@**�G�إߡB�����B�X�֡B�R���B���R�W����
- ? **����޲z**�G�����ܧ�B�ֳt����B�����^�_�B�������m
- ? **�Ȧs�\��**�G�Ȧs�ܧ�Bstash/pop�ާ@
- ? **���A�ˬd**�G�ԲӪ��x�s�w���A���R

#### ���Ť���\��
```csharp
// ����X�֡]�t�Ĭ��˴��^
public void MergeBranch(string branchName)

// �w������R���]�t�O�@����^  
public void DeleteBranch(string branchName)

// ���䭫�R�W�]�t���ҡ^
public void RenameBranch(string oldName, string newName)

// �q����إߤ���
public void CreateBranchFromCommit(string branchName, string commitSha)

// �����T�d��
public BranchInfo GetBranchInfo(string branchName)
```

### 2. ?? ����ϧκ޲z��

#### ��ı�ƥ\��
- ? **1400��900 �X�i����**�G���Ѽe�����ϧήi�ܪŶ�
- ? **�T�榡����**�G320px�������O + �����ϧΰ� + 380px�k�����O
- ? **���z�����ѧO**�G�D����B�\�����B�״_���䵥�����ѧO
- ? **�ϧΤƮi��**�G�`�I�B�s���u�B������Ҫ��M�~�i��

#### �椬�\��
```csharp
// �����ܩM�ާ@
private void OnBranchSelected(string branchName)
private void UpdateBranchActionButtons()

// �ϧ�ø�s
private void CreateBranchGraph()
private void DrawConnectionLines()
private void DrawBranchLabels()

// ����޲z�ާ@
private async void MergeBranchButton_Click()
private async void DeleteBranchButton_Click()
private async void RenameBranchButton_Click()
```

### 3. ?? �Τ@�`��D�D�t��

#### ����D�D�]�w
```xml
<!-- App.xaml -->
<Application RequestedTheme="Dark">
    <!-- �۰����β`��D�D��Ҧ����� -->
</Application>
```

#### ��m�]�p�W�d
| �h�� | �C��� | �γ~ |
|------|--------|------|
| **�D�I��** | `#111827` | �����D�I�� |
| **���O�I��** | `#1E293B` | ���e���O |
| **�d���I��** | `#334155` | ����I�� |
| **�D�n��r** | `#F1F5F9` | ���D���e |
| **���\��** | `#10B981` | ���\�ާ@ |
| **ĵ�i��** | `#3B82F6` | ��T���� |
| **�M�I��** | `#F87171` | �M�I�ާ@ |

### 4. ?? �Τ�޲z�t��

#### �n�J�t��
- ? **�Τ���**�G�U�ԦC���ܤw���U�Τ�
- ? **�s�Τ���U**�G�ʺA�s�W�Τ�\��
- ? **�]�w���[��**�GJSON�ɮ׫O�s�Τ���
- ? **�`��D�D**�G�Τ@����ı�]�p

#### �Τ�]�w�޲z
```csharp
public class UserSettings
{
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string LastRepositoryPath { get; set; } = string.Empty;
    public string AutoCADPath { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
```

### 5. ?? CAD�ɮ״��z�B�z

#### CAD�ɮת��A�˴�
```csharp
public class CadFileStatus
{
    public string FilePath { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsLocked { get; set; }  // �ɮ���w�˴�
}
```

#### AutoCAD��X�\��
- ? **�ɮ���w�˴�**�G�קK���楿�b�ϥΪ�CAD�ɮ�
- ? **�������**�G�ϥ�AutoCAD�}�Ҥ��P�����i����
- ? **���|�]�w**�G�۰ʰ����Τ�ʳ]�wAutoCAD���|
- ? **�妸�ާ@**�G���z���L��w�ɮת��Ȧs�ާ@

### 6. ? ����Git�\��

#### �����^�_�t��
```csharp
// �w���^�_�]�O�d���v�^
public void RevertToCommit(string commitSha, string authorName, string authorEmail)

// �M�I���m�]�R�����v�^
public void ResetToCommit(string commitSha)
```

#### �ֳt����
```csharp
// �@�䦡����y�{
private void QuickCommit()
{
    // �۰ʼȦs �� ���� �� ���s��z
    var stageResult = _gitService.StageAllChangesWithResult();
    _gitService.Commit(message, AuthorName, AuthorEmail);
    RefreshData();
}
```

#### �E�_�\��
```csharp
// ���z�E�_������D
private void DiagnoseCommitIssues()
{
    // �ˬd�G�x�s�w���A�B�@�̸�T�B�ɮת��A�BCAD�ɮ���w
}
```

---

## ??? �޳N�[�c�`��

### �M�׵��c
```
GitDWG/
�u�w�w Models/                    # ��Ƽҫ�
�x   �u�w�w CommitInfo.cs         # �����T
�x   �u�w�w AppUser.cs            # �Τ�ҫ�
�x   �|�w�w UserSettings.cs       # �Τ�]�w
�u�w�w Services/                  # �~�ȪA��
�x   �u�w�w GitService.cs         # Git�֤ߪA�ȡ]�t����޲z�^
�x   �u�w�w UserSettingsService.cs # �Τ�]�w�A��
�x   �|�w�w AutoCadCompareService.cs # AutoCAD��X�A��
�u�w�w Views/                     # �ϥΪ̤���
�x   �u�w�w BranchGraphWindow.cs  # ����ϧκ޲z��
�x   �u�w�w UserLoginWindow.cs    # �Τ�n�J����
�x   �|�w�w InitialSetupWindow.cs # ��l�]�w����
�u�w�w ViewModels/               # MVVM���ϼҫ�
�x   �|�w�w MainViewModel.cs      # �D���ϼҫ�
�u�w�w MainWindow.xaml(.cs)      # �D����
�|�w�w App.xaml(.cs)            # ���ε{���i�J�I
```

### �֤ߨ̿�
- **.NET 8.0**: �{�N��.NET���x
- **WinUI 3**: Windows���ε{��UI�ج[
- **LibGit2Sharp**: Git�ާ@�֤ߨ禡�w
- **�`��D�D**: �@���M�~�]�p

---

## ?? �\�৹����ˬd

### Git�������� ?
- [x] �x�s�w��l�ƩM�޲z
- [x] �ɮ׼Ȧs�M����
- [x] ����إߡB�����B�X�֡B�R���B���R�W
- [x] ������v�d�ݩM�����^�_
- [x] �i��Git�ާ@�]stash/pop�Breset���^

### CAD�ɮ׳B�z ?
- [x] CAD�ɮת��A�˴�
- [x] �ɮ���w�˴��M�B�z
- [x] AutoCAD��X�M�������
- [x] ���z���L����

### �Τ����� ?
- [x] �Τ�n�J�M�޲z�t��
- [x] ����ϧΤƺ޲z��
- [x] �Τ@�`��D�D�]�p
- [x] ���[���ާ@�ɭ�
- [x] �ԲӪ����~���ܩM�E�_

### �ʯ�Mí�w�� ?
- [x] ���`�B�z�M���~��_
- [x] �O����޲z�M�귽����
- [x] �j���x�s�w�į��u��
- [x] �T����UI�]�p

---

## ?? �U�@�B��ĳ

### 1. �ϥܸ귽
- �ݭn�ǳ�18133.png�t�C�ϥ��ɮ�
- ��ĳ�ؤo�G16��16, 24��24, 32��32, 44��44, 48��48, 150��150, 256��256, 310��150, 620��300
- �i�ҥ�GitDWG.csproj���Q���Ѫ��ϥܳ]�w

### 2. ���թM����
- ��ĳ�b�u�ꪺCAD�M�����Ҥ�����
- ���һP���P����AutoCAD���ۮe��
- ���դj���x�s�w���į��{

### 3. ���ɧ���
- �w���ѧ��㪺�ϥΫ��nmarkdown���
- �i�ھڹ�ڨϥΦ^�X�i�@�B����

---

## ?? �O�d�����n���

�H�U�O�w�M�z��O�d�����n���ɡG

1. **README.md** - �D�n�M�׻���
2. **GitDWG_v1.3.0_�ɯŧ������i.md** - �Բӥ\����i
3. **�ϧΤƤ���޲z���ϥΫ��n.md** - ����޲z���ϥλ���
4. **�ֳt����\��ϥΫ��n.md** - �ֳt����\�໡��
5. **�����^�_�\��ϥΫ��n.md** - �����^�_�\�໡��
6. **�Τ�n�J�t�ΨϥΫ��n.md** - �Τ�t�Ψϥλ���
7. **�s���ϥλ���.md** - ����ϥλ���

---

## ?? �̲��`��

**GitDWG v1.3.0 �{�w�����ǳƴN���I**

? **�֤ߦ��N**�G
- ?? **�M�~�Ť���ϧκ޲z��** - 1400��900�e�����ϡA�������ާ@
- ?? **����Git�\��䴩** - �q��¦�찪�Ū�Git��������
- ?? **�Τ�͵��t��** - �n�J�޲z�B�]�w���[��
- ?? **�Τ@�`��D�D** - �@���M�~�]�p
- ?? **CAD�ɮ״��z�B�z** - AutoCAD��X�B�ɮ���w�˴�

**�M�׫ظm���A�G? ���\**  
**�֤ߥ\��G? ����**  
**UI�D�D�G? �Τ@�`��**  
**���ɡG? �������**  

**GitDWG�{�b�O�@�ӥ\�৹��B�M�~�]�p��CAD��������u��I** ??