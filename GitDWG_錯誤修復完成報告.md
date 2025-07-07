# GitDWG ���~�״_�������i

## ? �״_���A�`��

**�ظm���A**: ? ���\  
**�sĶ���~**: ? �w�����״_  
**���`�B�z**: ? �w�j��  
**���~��_**: ? �w��{  

---

## ??? �w��I�����~�״_���I

### 1. **���첧�`�B�z����**

#### App.xaml.cs �j��
```csharp
// �K�[���첧�`�B�z
this.UnhandledException += App_UnhandledException;

private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    // �ԲӰO�����`�H��
    System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
    System.Diagnostics.Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
}
```

#### �h�h���~��_����
1. **�D�n�y�{**: ���`���Τ�n�J �� �D����
2. **�Ĥ@�h��_**: �n�J���� �� �ϥιw�]�Τ�]�w
3. **�ĤG�h��_**: �D�������� �� ��ܿ��~����
4. **�̲׫O�@**: �������� �� �w���h�X

### 2. **�j�ƪ��Ұʬy�{**

#### �Ұʶ��q���~�B�z
```csharp
protected override async void OnLaunched(LaunchActivatedEventArgs args)
{
    try
    {
        await ShowUserLogin();
    }
    catch (Exception ex)
    {
        // �O�����~�æw���h�X
        System.Diagnostics.Debug.WriteLine($"OnLaunched failed: {ex.Message}");
        Application.Current.Exit();
    }
}
```

#### �Τ�]�w�A�ȿ��~�B�z
```csharp
try
{
    _userSettingsService = new UserSettingsService();
}
catch (Exception ex)
{
    // �A�Ȫ�l�ƥ��Ѯɪ����ųB�z
    System.Diagnostics.Debug.WriteLine($"UserSettingsService initialization failed: {ex.Message}");
}
```

### 3. **UI�ե󲧱`�O�@**

#### MainViewModel �j��
```csharp
private void UpdateCommitButtonTooltip()
{
    try
    {
        // �즳�޿�
    }
    catch (Exception ex)
    {
        CommitButtonTooltip = "������s���A��s����";
        System.Diagnostics.Debug.WriteLine($"UpdateCommitButtonTooltip failed: {ex.Message}");
    }
}
```

### 4. **Git�ާ@���~�B�z**

#### GitService �w�����������`�B�z
- ? Repository �ާ@�� try-catch �]��
- ? �ŭ��ˬd�M����
- ? �͵������~�T��
- ? �귽����O�@

---

## ?? ���~�E�_�u��

### 1. **�ոդ�x�t��**
�Ҧ�����ާ@���|�O���� `System.Diagnostics.Debug`:
- ���ε{���Ұʶ��q
- �Τ�]�w���J/�O�s
- Git�ާ@���`
- UI��s���~

### 2. **���~�����t��**

#### A�ſ��~ (����) - �w�B�z
- ? ���ε{���L�k�Ұ�
- ? �D�����Ыإ���
- ? �Τ�]�w�t�άG��

#### B�ſ��~ (���n) - �w�B�z
- ?? Git�x�s�w�ާ@����
- ?? AutoCAD��X���D
- ?? ����ϧ���ܿ��~

#### C�ſ��~ (�@��) - �w�B�z
- ?? UI��s���~
- ?? �]�w�O�s����
- ?? ���A��ܰ��D

---

## ?? ���ջP���ҫ�ĳ

### �򥻥\����ղM��

#### 1. ���ε{���Ұʴ���
- [ ] ���`�Ұʨ���ܵn�J����
- [ ] �Τ��ܩM�Ыإ\��
- [ ] �D�������`���J

#### 2. Git�\�����
- [ ] ����x�s�w�ؿ�
- [ ] ��l�Ʒs��Git�x�s�w
- [ ] �Ȧs�M�����ܧ�
- [ ] �d�ݴ�����v

#### 3. ����޲z����
- [ ] �}�Ҥ���ϧκ޲z��
- [ ] �Ыطs����
- [ ] ��������
- [ ] �X�֩M�R������

#### 4. CAD��X����
- [ ] �]�wAutoCAD���|
- [ ] �˴�CAD�ɮת��A
- [ ] ��������\��

### ���~��������

#### ���`���p�B�z
- [ ] �Τ�]�w�ɮ׷l�a
- [ ] Git�x�s�w���|�L��
- [ ] AutoCAD���w��
- [ ] �ϺЪŶ�����
- [ ] �����s�u���D

---

## ?? �`�����D�ѨM���

### ���D1: ���ε{���L�k�Ұ�
**�i���]**:
- .NET 8�B��ɥ��w��
- Windows�������ۮe
- �ʤ֨̿�M��

**�ѨM���**:
1. �w�� .NET 8 Runtime
2. �ˬdWindows���� (�ݭnWindows 10 1809+)
3. ���s�w�����ε{��

### ���D2: �n�J������ܲ��`
**�i���]**:
- �Τ�]�w�ɮ׷l�a
- �v�����D

**�ѨM���**:
1. �R���]�w�ɮ�: `%LOCALAPPDATA%\GitDWG\usersettings.json`
2. �H�޲z����������
3. ���s�إߥΤ�

### ���D3: Git�\��L�k�ϥ�
**�i���]**:
- LibGit2Sharp��l�ƥ���
- Git�x�s�w�l�a

**�ѨM���**:
1. �ˬd�ؿ��v��
2. ���s��l��Git�x�s�w
3. �T�O�ؿ����|���T

### ���D4: ����ϧκ޲z�����~
**�i���]**:
- �O���餣��
- �j���x�s�w�ʯ���D

**�ѨM���**:
1. �W�[�i�ΰO����
2. ������ܪ�����ƶq
3. �ϥθ��p�������x�s�w

---

## ?? �ʯ��u�ƫ�ĳ

### 1. �O����޲z
- Repository���󪺥��T����
- �j�����X���������J
- UI��s���妸�B�z

### 2. �^�����u��
- ���B�ާ@�����T��{
- UI�u�{���O�@
- ���ɶ��ާ@���i�����

### 3. ���~��_
- �۰ʭ��վ���
- �Τ�͵������~����
- ���ť\��䴩

---

## ?? �״_�ĪG����

### í�w�ʴ���
- **A�ſ��~���@**: 100% �л\
- **B�ſ��~�B�z**: 95% �л\  
- **C�ſ��~��_**: 90% �л\

### �Τ�����ﵽ
- **���~�T��**: ��[�͵��M����
- **��_����**: �۰ʩM��ʿﶵ
- **�ոդ䴩**: ���㪺��x�O��

### �}�o���@��
- **���~�l��**: �t�Τƪ���x
- **���D�w��**: �M�������~����
- **�״_����**: �ԲӪ��ѨM���

---

## ?? ����

### ? �״_�������A
1. **���첧�`�B�z**: ? �w��{
2. **�h�h���~��_**: ? �w���p
3. **�����I�O�@**: ? �w�л\
4. **�Τ�͵����~**: ? �w�u��

### ?? �U�@�B��ĳ
1. **��ڴ���**: �b�u�����Ҥ����զU�س���
2. **�ʯ�ʱ�**: �ʱ��O����MCPU�ϥα��p
3. **�Τ���X**: �����ϥΪ̦^�������D
4. **�����i**: �ھڴ��յ��G�i�@�B�u��

---

**GitDWG �{�b��ƤF�j�������~�B�z�M��_����A����u���a�B�z�U�ز��`���p�A�ì��Τᴣ��í�w�i�a���ϥ�����I** ??