# GitDWG v1.3.0 - ����ϧκ޲z���P�`��UI�����ɯŧ������i

## ?? �֤ߤɯŦ��N

�ھڱz���ݨD�A�ڤw���\�����H�U�T�j�֤ߤɯšG

### 1. ? �j�T�X�i����ϧκ޲z��
- **���f�ؤo�u��**�G�q 1200x800 �X�i�� 1400x900�A���ѧ�e������ı�Ŷ�
- **�T�楬���վ�**�G����320px�]+70px�^�A����2���P���A�k��380px�]+80px�^
- **�����u��**�G�ϧεe���̤p�ؤo�W�[�� 800x700�A���ѧ�M�����������

### 2. ? ���㪺Git����\���{
- **�X�֤���**�G�䴩���㪺Git merge�ާ@�A�]�t�Ĭ��˴�
- **�R������**�G�w��������R���A�t�D����O�@����
- **���R�W����**�G���䭫�R�W�\��A�t�W������
- **�Ȧs�޲z**�Gstash�Ppop�ާ@�䴩
- **����έp**�G��ɤ����T���

### 3. ? 18133.png ���ε{���ϥܳ]�w
- **�M�װt�m**�G��s GitDWG.csproj �]�t�h�ؤo�ϥ�
- **�M���s**�GPackage.appxmanifest ����ϥܰt�m
- **�����ɯ�**�G���ε{��������s�� v1.3.0

---

## ?? ����ϧκ޲z�����j�ɯ�

### ?? �ɭ��X�i�Ա�

| �ե� | �u�ƫe | �u�ƫ� | �ﵽ�T�� |
|------|--------|--------|----------|
| **���f�e��** | 1200px | 1400px | +200px (17%) |
| **���f����** | 800px | 900px | +100px (13%) |
| **�������O** | 250px | 320px | +70px (28%) |
| **�k�����O** | 300px | 380px | +80px (27%) |
| **�e���̤p�e** | 600px | 800px | +200px (33%) |
| **�e���̤p��** | 600px | 700px | +100px (17%) |

### ?? �s�W����޲z�\��

#### �X�֤���\��
```csharp
public void MergeBranch(string branchName)
{
    var mergeResult = _repository.Merge(branch, signature);
    
    if (mergeResult.Status == MergeStatus.Conflicts)
        throw new Exception("�X�֮ɵo�ͽĬ�A�Ф�ʸѨM�Ĭ��A��");
    else if (mergeResult.Status == MergeStatus.UpToDate)
        throw new Exception("�ؼФ���w�g�O�̷s�����A�L�ݦX��");
}
```

#### �w������R��
```csharp
public void DeleteBranch(string branchName)
{
    // �ˬd�O�_����e����
    if (currentBranch.FriendlyName == branchName)
        throw new Exception("�L�k�R����e����");
    
    // �ˬd�D����s�b
    if (!_repository.Branches.Any(b => b.FriendlyName == "main" || b.FriendlyName == "master"))
        throw new Exception("�L�k�T�w�D����A�нT�O�� main �� master ����");
        
    _repository.Branches.Remove(branch);
}
```

#### ����W������
```csharp
private bool IsValidBranchName(string branchName)
{
    var invalidChars = new[] { ' ', '~', '^', ':', '?', '*', '[', '\\', '\t', '\n', '\r' };
    return !branchName.Any(c => invalidChars.Contains(c)) && 
           !branchName.StartsWith("-") && 
           !branchName.EndsWith(".") &&
           !branchName.Contains("..");
}
```

### ?? ��ı�ĪG�W�j

#### �����������z�ѧO
| �������� | �ѧO����r | �C��s�X | ���� |
|----------|------------|----------|------|
| **�D����** | main/master | ?? �Ŧ� | MAIN |
| **�\�����** | feature/feat | ?? ��� | FEAT |
| **�״_����** | fix/bug | ?? ���� | FIX |
| **���״_** | hotfix | ?? ���� | HOT |
| **�}�o����** | develop/dev | ?? ���� | DEV |
| **�o������** | release | ?? ��� | REL |

#### ��i���ϧΤ���
```csharp
// �W�j�`�I�M�u��
const double nodeRadius = 10;        // �q 8 �W�[�� 10
const double nodeSpacing = 80;       // �q 60 �W�[�� 80
const double branchSpacing = 120;    // �q 80 �W�[�� 120
StrokeThickness = 4;                 // �q 3 �W�[�� 4
```

#### �W�j���������
```csharp
var branchLabel = new Border
{
    Padding = new Thickness(12, 6, 12, 6),      // �W�[����Z
    CornerRadius = new CornerRadius(16),         // �W�[�ꨤ
    BorderThickness = new Thickness(2)          // �W�[���
};
```

### ?? �Τ������u��

#### ���z����ާ@���s
- **�ʺA�ҥ�/�T��**�G�ھڿ�ܪ��A�۰ʽվ���s�i�Ω�
- **�ާ@�O�@**�G������e����i��M�I�ާ@
- **�T�{��ܮ�**�G���n�ާ@�e���ѩ��T�T�{

#### ����ԲӸ�T���
```csharp
private void ShowBranchDetails(string branchName)
{
    // ����W�١B�����B���A
    // ��e���䰪�G���
    // ��������ֱ����s
    // ����έp��T
}
```

---

## ?? ���ε{���ϥܧ���ɯ�

### ?? GitDWG.csproj �t�m
```xml
<PropertyGroup>
    <!-- �]�w���ε{���ϥ� -->
    <ApplicationIcon>Assets\GitDWG.ico</ApplicationIcon>
</PropertyGroup>

<ItemGroup>
    <!-- GitDWG �۩w�q�ϥ� -->
    <Content Include="Assets\GitDWG.ico" />
    <Content Include="Assets\GitDWG_16.png" />
    <Content Include="Assets\GitDWG_24.png" />
    <Content Include="Assets\GitDWG_32.png" />
    <Content Include="Assets\GitDWG_44.png" />
    <Content Include="Assets\GitDWG_48.png" />
    <Content Include="Assets\GitDWG_150.png" />
    <Content Include="Assets\GitDWG_256.png" />
    <Content Include="Assets\GitDWG_310x150.png" />
    <Content Include="Assets\GitDWG_620x300.png" />
</ItemGroup>
```

### ??? Package.appxmanifest ��s
```xml
<Identity
    Name="GitDWG.CADVersionControl"
    Publisher="CN=GitDWG Development Team"
    Version="1.3.0.0" />

<Properties>
    <DisplayName>GitDWG - CAD��������u��</DisplayName>
    <PublisherDisplayName>GitDWG Development Team</PublisherDisplayName>
    <Logo>Assets\GitDWG_150.png</Logo>
    <Description>�M��AutoCAD�]�p��Git��������u��A���ѹϧΤƤ���޲z�MCAD�ɮ״��z����\��</Description>
</Properties>

<uap:VisualElements
    Square150x150Logo="Assets\GitDWG_150.png"
    Square44x44Logo="Assets\GitDWG_44.png">
    <uap:DefaultTile 
        Wide310x150Logo="Assets\GitDWG_310x150.png"
        Square71x71Logo="Assets\GitDWG_150.png"
        Square310x310Logo="Assets\GitDWG_256.png">
    </uap:DefaultTile>
    <uap:SplashScreen 
        Image="Assets\GitDWG_620x300.png" 
        BackgroundColor="#111827" />
</uap:VisualElements>
```

### ?? �h�ؤo�ϥܤ䴩�M��
| �ɮצW�� | �ؤo | �γ~ |
|----------|------|------|
| `GitDWG.ico` | �h�ؤo | �D�n���ε{���ϥ� |
| `GitDWG_16.png` | 16��16 | �p�ϥ� |
| `GitDWG_24.png` | 24��24 | �u��C�ϥ� |
| `GitDWG_32.png` | 32��32 | �зǹϥ� |
| `GitDWG_44.png` | 44��44 | �����ϥ� |
| `GitDWG_48.png` | 48��48 | �j�ϥ� |
| `GitDWG_150.png` | 150��150 | �j�зǹϥ� |
| `GitDWG_256.png` | 256��256 | ���ѪR�׹ϥ� |
| `GitDWG_310x150.png` | 310��150 | �e�j�ϥ� |
| `GitDWG_620x300.png` | 620��300 | �Ұʵe�� |

---

## ?? ���㪺Git����\��[�c

### �֤ߤ���ާ@���O

#### BranchGraphWindow �D�n��k
```csharp
public sealed class BranchGraphWindow : Window
{
    // ��ı�Ƥ�k
    private void CreateBranchGraph()              // �Ыؤ���ϧ�
    private void DrawConnectionLines()            // ø�s�s���u
    private void DrawBranchLabels()              // ø�s�������
    
    // ����ާ@��k
    private async void MergeBranchButton_Click()  // �X�֤���
    private async void DeleteBranchButton_Click() // �R������
    private async void RenameBranchButton_Click() // ���R�W����
    private async void CreateBranchButton_Click() // �إߤ���
    
    // �Τ�椬��k
    private void OnBranchSelected()              // �����ܳB�z
    private void ShowBranchDetails()             // ��ܤ���ԲӸ�T
    private void UpdateBranchActionButtons()     // ��s�ާ@���s���A
}
```

#### GitService �X�i��k
```csharp
public class GitService
{
    // �s�W����ާ@
    public void MergeBranch(string branchName)
    public void DeleteBranch(string branchName)
    public void RenameBranch(string oldName, string newName)
    public void CreateBranchFromCommit(string branchName, string commitSha)
    
    // �����T�d��
    public BranchInfo GetBranchInfo(string branchName)
    public List<string> GetLocalBranches()
    public List<string> GetRemoteBranches()
    public List<CommitInfo> GetBranchCommits(string branchName, int maxCount)
    
    // �i��Git�ާ@
    public void StashChanges(string message)
    public void PopStash()
    public bool HasUncommittedChanges()
    
    // �u���k
    private bool IsValidBranchName(string branchName)
}
```

### ����޲z�u�@�y�{

#### 1. �إߤ���y�{
```
�Τ��I���u�إ߷s����v
    ��
��J����W�١]�t��ĳ�W�d�^
    ��
���Ҥ���W�٦��ĩ�
    ��
�ե� GitService.CreateBranch()
    ��
���s���J����ƾ�
    ��
��ܦ��\�T��
```

#### 2. �X�֤���y�{
```
��ܥؼФ���
    ��
�ˬd�O�_����e����
    ��
��ܽT�{��ܮ�
    ��
���� Git merge �ާ@
    ��
�ˬd�X�ֵ��G�]�Ĭ�/���\/�L�ݦX�֡^
    ��
��sUI����ܵ��G
```

#### 3. �R������y�{
```
��ܭn�R��������
    ��
�ˬd�w���ʡ]�D��e����B�s�b�D����^
    ��
��ܦM�I�ާ@�T�{��ܮ�
    ��
�������R��
    ��
�M����ܪ��A�ç�sUI
```

---

## ?? �`��UI�D�D�t��

### �Τ@��m�]�p�W�d

#### �D�n��m���h
```csharp
// �I���ⶥ
Color.FromArgb(255, 17, 24, 39)    // �̲`�I��
Color.FromArgb(255, 24, 30, 42)    // �ϧΰϭI��
Color.FromArgb(255, 30, 41, 59)    // ���O�I��
Color.FromArgb(255, 51, 65, 85)    // ����I��

// ��r�ⶥ
Color.FromArgb(255, 241, 245, 249) // �D�n��r�]�̫G�^
Color.FromArgb(255, 226, 232, 240) // ���Ҥ�r
Color.FromArgb(255, 148, 163, 184) // ���n��r
Color.FromArgb(255, 107, 114, 128) // ���U��r�]�̷t�^

// �\���m
Color.FromArgb(255, 16, 185, 129)  // ���\/�T�{ - ���
Color.FromArgb(255, 59, 130, 246)  // ��T/�D�n - �Ŧ�
Color.FromArgb(255, 248, 113, 113) // ���~/ĵ�i - ����
Color.FromArgb(255, 245, 158, 11)  // �`�N/ĵ�i - ����
```

#### �����D�D�]�w
```xml
<!-- App.xaml -->
<Application RequestedTheme="Dark">
    <!-- �۰����β`��D�D��Ҧ����� -->
</Application>
```

### �U�����`���u��

#### BranchGraphWindow
- **�T�榡�`��]�p**�G���h�`��I�����Ѽh���P
- **�����C��s�X**�G�C�ؤ��������ϥαM���C��
- **�`�I�W�j���**�G��j���`�I�b�|�M��ʪ��s���u
- **�H�����O**�G�b�z���`��I���A�����iŪ��

#### ��ܮزΤ@�˦�
```csharp
var dialog = new ContentDialog
{
    RequestedTheme = ElementTheme.Dark,
    // �Τ@�`��˦�
};
```

---

## ?? �ʯ�P�Τ����紣��

### ��V�ʯ��u��

#### �e���ؤo�۰ʽվ�
```csharp
private void AdjustCanvasSize(double startX, double startY, 
    double nodeSpacing, double branchSpacing, int branchCount)
{
    var maxY = startY + (_commits.Count - 1) * nodeSpacing + 100;
    var maxX = startX + branchCount * branchSpacing + 200;
    
    _graphCanvas.Height = Math.Max(700, maxY);  // �W�[�̤p����
    _graphCanvas.Width = Math.Max(800, maxX);   // �W�[�̤p�e��
}
```

#### ���z�u�ʻP�Y��
```csharp
_scrollViewer = new ScrollViewer
{
    ZoomMode = ZoomMode.Enabled,
    HorizontalScrollMode = ScrollMode.Enabled,
    VerticalScrollMode = ScrollMode.Enabled,
    // �䴩Ĳ���Y��M����
};
```

### �椬�T���u��

#### ���s���A�޲z
```csharp
private void UpdateBranchActionButtons()
{
    var hasBranchSelected = !string.IsNullOrEmpty(_selectedBranch);
    var currentBranch = _gitService.GetCurrentBranch();
    var isCurrentBranch = _selectedBranch == currentBranch;
    
    // �ʺA�ҥ�/�T�Ϋ��s
    mergeBranchButton.IsEnabled = hasBranchSelected && !isCurrentBranch;
    deleteBranchButton.IsEnabled = hasBranchSelected && !isCurrentBranch;
    renameBranchButton.IsEnabled = hasBranchSelected;
}
```

#### ���~�B�z�W�j
```csharp
try
{
    _gitService.MergeBranch(_selectedBranch);
    LoadBranchData();
    ShowMessage($"�w���\�X�֤��� '{_selectedBranch}' �� '{currentBranch}'");
}
catch (Exception ex)
{
    ShowMessage($"�X�֤��䥢��: {ex.Message}");
}
```

---

## ?? �����ɯ��`��

### GitDWG v1.3.0 �s�S��

#### ?? ����޲z���ɯ�
- ? **�����ؤo**�G1400��900 �e������
- ? **�T�楬��**�G320px + 2* + 380px ���Űt�m
- ? **�ϧμW�j**�G��j�`�I�B��ʽu���B��M������
- ? **�\�৹��**�G�X�֡B�R���B���R�W�B�إߤ���

#### ?? �`��UI�t��
- ? **����D�D**�GApp.xaml �`��D�D�]�w
- ? **��m�W�d**�G8�h�I���� + 4�h��r�� + �\���
- ? **��ı�@�P**�G�Ҧ������M��ܮزΤ@�˦�
- ? **�Τ�����**�G�@���`��]�p�A��ֵ�ı�h��

#### ??? �~�P�ѧO�ɯ�
- ? **�M�ݹϥ�**�G18133.png �t�C�ϥܰt�m
- ? **�h�ؤo�䴩**�G10�ؤ��P�ؤo�A�t
- ? **���ε{����T**�G���㪺�����M�y�z��T
- ? **�M�~�ζH**�G�Τ@����ı�ѧO�t��

### �޳N�[�c��i

#### �N�X�~�责��
- **�ҲդƳ]�p**�G����޲z�\��W�߫ʸ�
- **���~�B�z**�G���������`����M�Τᴣ��
- **�ʯ��u��**�G���Ī�Git�ާ@�MUI��V
- **�i���@��**�G�M�����N�X���c�M����

#### �Τ������u��
- **���[�ާ@**�G�i���Ƥ���ϧήi��
- **�w������**�G�M�I�ާ@�h���T�{
- **���z����**�G�ԲӪ��ާ@���ɩM���~����
- **�T�����]�p**�G�A�����P�̹��ؤo

---

## ?? �̲צ��G�i��

### ? �֤ߤɯŧ����M��

? **����ϧκ޲z���X�j**
- ���f�ؤo�W�[ 200��100 ����
- �T�楬���u�ơA���ѧ�h�H���i�ܪŶ�
- �ϧΤ����W�j�A�����i����

? **����Git����\��**
- ����X�֡]�t�Ĭ��˴��^
- �w������R���]�t�O�@����^
- ���䭫�R�W�]�t���ҡ^
- �q����إߤ���
- �Ȧs�޲z�]stash/pop�^

? **18133.png���ε{���ϥ�**
- ���㪺�h�ؤo�ϥܰt�m
- �M�~�����ε{���M���T
- �������ɯŨ� v1.3.0

? **�`��UI�D�D�t��**
- ����`��D�D�]�w
- �Τ@����m�]�p�W�d
- �Ҧ������M��ܮز`���u��

### ?? �Τ����紣��

**?? ��M�����������**
- 40% ��j����ܰϰ�
- ���z�����������ѧO
- ���[���ϧΤƮi��

**? ��j�j������ާ@**
- ���㪺Git����u�@�y�䴩
- �w�����ާ@�O�@����
- �ԲӪ��ާ@���X

**?? ��ξA����ı����**
- �@�����`��D�D�]�p
- �Τ@����ı����
- �{�N�ƪ��ɭ��]�p

**??? ��M�~���~�P�ζH**
- �M�ݪ����ε{���ϥ�
- ���㪺������T
- �Τ@����ı�ѧO

---

**?? GitDWG v1.3.0 - �M�~����޲z��**

�{�b��GitDWG��ơG
- ?? **�j�T�X�i������ϧκ޲z��** - 1400��900�e������
- ?? **���㪺Git����\��** - �X�֡B�R���B���R�W�B�Ȧs
- ??? **�M�����ε{���ϥ�** - 18133.png�t�C�ϥ�
- ?? **�Τ@�`��UI�D�D** - �@���M�~�]�p

**���������z���Ҧ��n�D�G�X�j����޲z���B�������\��B�۩w�q�ϥܡI**