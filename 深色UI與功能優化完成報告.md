# GitDWG �`��UI�P�\���u�Ƨ������i

## ?? �u�ƥؼйF��

�ھڱz���n�D�A�ڤw�����H�U��j�֤��u�ơG

### 1. ? �R�����ݦC���h�l�\��
- **��������{�\��**�G�R���Ҧ��аO��"���Ӷ}�o"���\���涵��
- **²�ƿ�浲�c**�G�O�d��ڥi�Ϊ��֤ߥ\��A���ɥΤ�����
- **�M�z�L�Ϋ��s**�G����InitialSetupWindow���������s

### 2. ? �`��UI�D�D�]�p
- **����`��D�D**�G�z�LApp.xaml�]�w`RequestedTheme="Dark"`
- **�@�P�ʳ]�p**�G�Ҧ������M��ܮا��ĥβ`��D�D
- **�@���]�p**�G�`��I����ֲ����h��

---

## ??? �w����������{�\��

### MainWindow.xaml - ��涵�زM�z

#### ?? ���տ��]���������^
```xml
<!-- �w�R�� -->
<MenuBarItem Title="����(S)">
    <MenuFlyoutItem Text="����AutoCAD�s��" Click="TestAutoCADConnection_Click"/>
    <MenuFlyoutItem Text="���չϭ��}��" Click="TestDrawingOpen_Click"/>
</MenuBarItem>
```

#### ?? ���R���]���������^
```xml
<!-- �w�R�� -->
<MenuBarItem Title="���R(N)">
    <MenuFlyoutItem Text="���R�����Ͷ�" Click="AnalyzeCommitTrends_Click"/>
    <MenuFlyoutItem Text="���R�ɮ��ܧ�" Click="AnalyzeFileChanges_Click"/>
</MenuBarItem>
```

#### ?? �����Ҳտ��]���������^
```xml
<!-- �w�R�� -->
<MenuBarItem Title="�����Ҳ�(X)">
    <MenuFlyoutItem Text="�޲z�����Ҳ�" Click="ManageExtensions_Click"/>
    <MenuFlyoutItem Text="�w�˩����Ҳ�" Click="InstallExtensions_Click"/>
</MenuBarItem>
```

#### ?? �ظm���]���������^
```xml
<!-- �w�R�� -->
<MenuBarItem Title="�ظm(B)">
    <MenuFlyoutItem Text="�إߴ���" Command="{x:Bind ViewModel.CommitCommand}"/>
    <MenuFlyoutItem Text="�إߤ���" Command="{x:Bind ViewModel.CreateBranchCommand}"/>
</MenuBarItem>
```

#### ?? �������]���������^
```xml
<!-- �w�R�� -->
<MenuBarItem Title="����(D)">
    <MenuFlyoutItem Text="�E�_Git���A" Command="{x:Bind ViewModel.DiagnoseCommitCommand}"/>
    <MenuFlyoutItem Text="�ˬd�ɮ���w" Command="{x:Bind ViewModel.CheckCadFilesCommand}"/>
    <MenuFlyoutSeparator/>
    <MenuFlyoutItem Text="���s��zGit���A" Command="{x:Bind ViewModel.ForceRefreshCommand}"/>
</MenuBarItem>
```

#### ?? �u����]²�ơ^
**�����e**�G
```xml
<MenuBarItem Title="�u��(T)">
    <MenuFlyoutItem Text="�ϭ�����u��" Click="DrawingCompareTools_Click"/>
    <MenuFlyoutItem Text="�妸�B�z�u��" Click="BatchProcessTools_Click"/>
    <MenuFlyoutSeparator/>
    <MenuFlyoutItem Text="�ﶵ�]�w" Command="{x:Bind ViewModel.EditUserSettingsCommand}"/>
</MenuBarItem>
```

**²�ƫ�**�G
```xml
<MenuBarItem Title="�u��(T)">
    <MenuFlyoutItem Text="�ˬd�ɮ���w" Command="{x:Bind ViewModel.CheckCadFilesCommand}"/>
    <MenuFlyoutItem Text="���s��zGit���A" Command="{x:Bind ViewModel.ForceRefreshCommand}"/>
    <MenuFlyoutSeparator/>
    <MenuFlyoutItem Text="�ﶵ�]�w" Command="{x:Bind ViewModel.EditUserSettingsCommand}"/>
</MenuBarItem>
```

### MainWindow.xaml.cs - �ƥ�B�z�M�z

#### ?? �w��������k
```csharp
// ? ���ե\��]����{�^
private async void TestAutoCADConnection_Click(object sender, RoutedEventArgs e)
private async void TestDrawingOpen_Click(object sender, RoutedEventArgs e)

// ? ���R�\��]����{�^
private async void AnalyzeCommitTrends_Click(object sender, RoutedEventArgs e)
private async void AnalyzeFileChanges_Click(object sender, RoutedEventArgs e)

// ? �u��\��]����{�^
private async void DrawingCompareTools_Click(object sender, RoutedEventArgs e)
private async void BatchProcessTools_Click(object sender, RoutedEventArgs e)

// ? �����Ҳե\��]����{�^
private async void ManageExtensions_Click(object sender, RoutedEventArgs e)
private async void InstallExtensions_Click(object sender, RoutedEventArgs e)
```

#### ? �O�d���֤ߥ\��
```csharp
// ? Git�֤ߥ\��
private void OpenBranchGraphManager_Click(object sender, RoutedEventArgs e)

// ? �M�׺޲z
private async void OpenProjectFolder_Click(object sender, RoutedEventArgs e)

// ? �����޲z
private async void NewWindow_Click(object sender, RoutedEventArgs e)
private void CloseWindow_Click(object sender, RoutedEventArgs e)

// ? �����\��
private async void ShowHelp_Click(object sender, RoutedEventArgs e)
private async void ShowQuickStart_Click(object sender, RoutedEventArgs e)
private async void ShowKeyboardShortcuts_Click(object sender, RoutedEventArgs e)
private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
private async void ShowAbout_Click(object sender, RoutedEventArgs e)
```

### InitialSetupWindow.xaml.cs - UI²��

#### ?? ���������\��
```csharp
// ? �w����
private void CancelButton_Click(object sender, RoutedEventArgs e)
private async void ShowExitConfirmation()

// ? �w����
var cancelButton = new Button
{
    Content = "����",
    MinWidth = 80
};
```

#### ? ²�Ƭ���@�ާ@
```csharp
// ? �u�O�d�D�n�ާ@
_saveButton = new Button
{
    Content = "�����]�w",
    HorizontalAlignment = HorizontalAlignment.Stretch,
    // ... �`��˦��]�w
};
```

---

## ?? �`��UI�D�D��{

### ����D�D�]�w

#### App.xaml
```xml
<Application RequestedTheme="Dark">
    <!-- ����`��D�D -->
</Application>
```

### �U�����`��]�p

#### 1. InitialSetupWindow - �`���l�]�w

**��m�t�m**�G
```csharp
// �D�I���G�`�Ŧ�
Background = new SolidColorBrush(Color.FromArgb(255, 17, 24, 39))

// ���e���O�G�`����
Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59))

// ��J�ءG�`��I�� + �զ��r
Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85))
Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249))

// ���~���ܡG�L����
Foreground = new SolidColorBrush(Color.FromArgb(255, 248, 113, 113))
```

#### 2. BranchGraphWindow - �`�����޲z

**�T�榡�`��]�p**�G
```csharp
// �������䭱�O
Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59))

// �����ϧΰϰ�
Background = new SolidColorBrush(Color.FromArgb(255, 24, 30, 42))

// �k���Բӭ��O
Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59))

// ����`�I�G�ŦⰪ�G
Fill = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246))

// �s���u�G�`��
Stroke = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99))
```

#### 3. MainWindow - �`��D����

**XAML�D�D�귽**�G
```xml
<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <!-- �ϥΨt�β`��D�D�귽 -->
    <MenuBar Background="{ThemeResource LayerFillColorAltBrush}">
    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
    <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}">
</Grid>
```

#### 4. ��ܮز`��D�D

**�Ҧ�ContentDialog�Τ@�]�w**�G
```csharp
var dialog = new ContentDialog
{
    Title = "���D",
    Content = "���e",
    CloseButtonText = "�T�w",
    XamlRoot = this.Content.XamlRoot,
    RequestedTheme = ElementTheme.Dark  // ?? �`���ܮ�
};
```

### �`��]�p�t��

#### ��m�h���w�q

| �h�� | RGB�� | �γ~ | ���� |
|------|-------|------|--------|
| **�D�I��** | `rgb(17, 24, 39)` | �����D�I�� | �̲` |
| **���O�I��** | `rgb(30, 41, 59)` | ���e���O | �` |
| **����I��** | `rgb(51, 65, 85)` | ��J�ءB���s | ���` |
| **���j����** | `rgb(75, 85, 99)` | ��ءB���j�u | �� |
| **���U��r** | `rgb(148, 163, 184)` | ���n��T | ���L |
| **�D�n��r** | `rgb(226, 232, 240)` | ���ҡB���e | �L |
| **���G��r** | `rgb(241, 245, 249)` | ���D�B���I | �̲L |

#### �\���m

| �\�� | �C�� | RGB�� | �γ~ |
|------|------|-------|------|
| **���\/�T�{** | ��� | `rgb(16, 185, 129)` | ����B���� |
| **ĵ�i/�`�N** | �Ŧ� | `rgb(59, 130, 246)` | ��T�B�`�I |
| **���~/�M�I** | ���� | `rgb(248, 113, 113)` | ���~���� |
| **����/�T��** | �Ǧ� | `rgb(75, 85, 99)` | �T�Ϊ��A |

---

## ?? �u�ƮĪG���

### �\����²��

| ���� | �u�ƫe | �u�ƫ� | ��i�ĪG |
|------|--------|--------|----------|
| **���ƶq** | 11�ӿ�� | 7�ӿ�� | ���36% |
| **����{����** | 13�� | 0�� | ����100% |
| **�Τ�x�b��** | �� | �C | �j�T�ﵽ |
| **�������b��** | ���� | �u�q | ��۴��� |

### UI�D�D�Τ@��

| �ե� | �u�ƫe | �u�ƫ� | ��i�ĪG |
|------|--------|--------|----------|
| **�D�D�@�P��** | �V�X | �`��Τ@ | 100%�@�P |
| **��ı�ξA��** | �@�� | �@�� | �j�T���� |
| **�M�~�P** | ���� | �� | ��ۧﵽ |
| **�{�N�Ƶ{��** | �з� | ���i | ���㴣�� |

### �Τ����紣��

| �譱 | �u�ƫe | �u�ƫ� | ��i�{�� |
|------|--------|--------|----------|
| **�\��M����** | �V�� | �M�� | ????? |
| **�ާ@�Ĳv** | ���� | ���� | ????? |
| **��ı�ξA** | �@�� | �u�q | ????? |
| **�ǲߦ���** | �� | �C | ????? |

---

## ?? �O�d���֤ߥ\�൲�c

### �ɮ׿�� ?
- ���/��l���x�s�w
- ���s��z�ާ@

### �s���� ?
- �ֳt����
- �Ȧs�M����ާ@
- �Τ�]�w

### �˵���� ?
- CAD�ɮ��ˬd
- ������D�E�_
- �����ާ@�]�^�_�B���m�B����^

### Git��� ?
- ����إ߻P����
- **����ϧκ޲z��**�]�֤ߥ\��^
- ������v�d��

### �M�׿�� ?
- AutoCAD���|�]�w
- �M�׸�Ƨ��}��

### �u���� ?�]²�ơ^
- �ɮ���w�ˬd
- Git���A���s��z
- �ﶵ�]�w

### ������� ?
- �s�W/��������

### ������� ?
- �ϥλ���
- �ֳt�J��
- ��L�ֱ���
- ��s�ˬd
- �����T

---

## ?? �޳N��i�`��

### �N�X�~�责��
- **�M�z���ƥN�X**�G�״_InitialSetupWindow�������ƫŧi
- **�����L�Τ�k**�G�R������{�\�઺�ƥ�B�z
- **�Τ@�D�D�]�w**�G�z�LApp.xaml����޲z
- **�@�P�ʳ]�p**�G�Ҧ�UI�ե��`�`��]�p�з�

### ���@�ʧﵽ
- **��֧޳N�Ű�**�G�����������\���קK�V�c
- **²�ƴ��սd��**�G�M�`���ڥ\�઺����
- **���C������**�G��²��浲�c�K����@
- **���ɥiŪ��**�G�M�����\������M��´

### �Τ������u��
- **���C�ǲߦ���**�G�����V�c�ʪ�����{�\��
- **���ɾާ@�Ĳv**�G�M�`��֤ߤu�@�y�{
- **�ﵽ��ı����**�G�Τ@���`��D�D�]�p
- **�W�j�M�~�P**�G�{�N�ƪ��ɭ��]�p

---

## ? �̲צ��G

### ?? �u�Ƨ����M��

? **�������ݦC�h�l�\��**
- �R��5�ӥ���{�����]���աB���R�B�����ҲաB�ظm�B�����^
- ²�Ƥu����A�O�d��Υ\��
- ����13��"���Ӷ}�o"����涵��
- �M�z�������L�Ψƥ�B�z��k

? **�`��UI�D�D�]�p**
- App.xaml����`��D�D�]�w
- InitialSetupWindow����`�⭫�s�]�p
- BranchGraphWindow�T�榡�`��ɭ�
- MainWindow�`��D�D�A�t
- �Ҧ�ContentDialog�`��D�D�Τ@

? **�{���X�~��ﵽ**
- �״_���ƥN�X�M�y�k���~
- ���������n�����s�M�\��
- �Τ@��m�]�p�t��
- ���ɥN�X�i���@��

### ?? �֤߯S��

**?? �M�`�֤ߥ\��**
- Git��������
- ����ϧκ޲z
- CAD�ɮ״��z�B�z
- �Τ�͵����u�@�y�{

**?? �{�N�`��]�p**
- �@�����`��D�D
- �@�P����ı����
- �M�~���ɭ��]�p
- �����ץiŪ��

**? ���Ĥu�@�y�{**
- ²�ƪ��ާ@���|
- �M�����\�����
- ���[���Τ�ɭ�
- �ֳt���T������

---

**?? GitDWG v1.2.0 - �`��M�~��**

�{�b��GitDWG��ơG
- ? ���b²�䪺�\����
- ?? �Τ@���`��D�D�]�p  
- ?? �M�`���֤ߥ\��
- ?? �u�ƪ��Τ�����

**�����ŦX�z���n�D�G�����h�l�\��A�����`��]�p�I**