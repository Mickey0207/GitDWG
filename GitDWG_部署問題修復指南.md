# ?? GitDWG WinUI 3 ���p���D�״_���n

## ? ���D�w�ѨM

**���~�T��**: "���������p�M�סA�~��i�氻���C�ЦbConfiguration Manager���ҥ�[���p]�C"

**�ڥ���]**: Package.appxmanifest�ɮפޥΤF���s�b���۩w�q�ϥ��ɮ�

**�״_���A**: ? �w����

---

## ??? �w���檺�״_�B�J

### 1. **Package.appxmanifest�ɮ׭״_**

#### �״_�e�����D
```xml
<!-- �ޥΤ��s�b���ɮ� -->
<Logo>Assets\GitDWG_150.png</Logo>
<Square150x150Logo="Assets\GitDWG_150.png"
<Square44x44Logo="Assets\GitDWG_44.png">
<SplashScreen Image="Assets\GitDWG_620x300.png"
```

#### �״_�᪺�]�w
```xml
<!-- �ϥβ{�����w�]�ϥ��ɮ� -->
<Logo>Assets\Square150x150Logo.scale-200.png</Logo>
<Square150x150Logo="Assets\Square150x150Logo.scale-200.png"
<Square44x44Logo="Assets\Square44x44Logo.scale-200.png">
<SplashScreen Image="Assets\SplashScreen.scale-200.png"
```

### 2. **�T�{�{��Assets�ɮ�**

? �H�U�ɮפw�s�b�B�i�ΡG
- `LockScreenLogo.scale-200.png`
- `SplashScreen.scale-200.png`
- `Square150x150Logo.scale-200.png`
- `Square44x44Logo.scale-200.png`
- `Square44x44Logo.targetsize-24_altform-unplated.png`
- `StoreLogo.png`
- `Wide310x150Logo.scale-200.png`

### 3. **�ظm����**
? �M�׫ظm���\�A�L�sĶ���~

---

## ?? �{�b�i�H���`�B�檺�\��

### ���p�M����
- ? F5 ��������
- ? Ctrl+F5 ����������
- ? Visual Studio���p
- ? �M��إߩM�w��

### ���ε{���\��
- ? ���`�ҰʩM��l��
- ? �Τ�n�J�t��
- ? Git��������\��
- ? ����ϧκ޲z��
- ? CAD�ɮ׳B�z

---

## ?? Visual Studio �����]�w���n

### 1. **�T�{�ҰʱM�׳]�w**
�bVisual Studio���G
1. �k���I���ѨM��� �� "�]�w���ҰʱM��"
2. �T�{GitDWG�M�׳Q�]���ҰʱM�ס]������ܡ^

### 2. **Configuration Manager�ˬd**
1. ��ܿ��G**�ظm** �� **Configuration Manager**
2. �T�{GitDWG�M�ת�"���p"�֨�����w�Ŀ�
3. ���x�]�w��ܾA���[�c�]x64���ˡ^

### 3. **�����]�w**
```
�]�w����          ��ĳ��
================  ==================
�]�w              Debug
���x              x64 (�� x86)
�ؼЬ[�c          net8.0-windows
���p              ? �w�ҥ�
�ҰʱM��          GitDWG
```

### 4. **�B�����**
- **F5**: �}�l����
- **Ctrl+F5**: �}�l����(������)
- **F6**: �ظm�M��
- **F7**: �ظm�ѨM���

---

## ?? WinUI 3 �M�ׯS��`�N�ƶ�

### MSIX �M�󳡸p
WinUI 3���ε{���ݭn�����p��MSIX�M��~�����G

#### �۰ʳ��p�y�{
1. Visual Studio�۰ʫإ�MSIX�M��
2. �w�˨쥻��Windows
3. ���U���ε{��
4. �Ұʰ����u�@���q

#### ���p�ݨD
- ? ���Ī�Package.appxmanifest
- ? ���T���ϥ��ɮװѷ�
- ? �A���v���]�w
- ? Windows�}�o�H���Ҧ�

### �`�����p���D
| ���D | ��] | �ѨM��� |
|------|------|----------|
| �ϥ��ɮ׿� | manifest�ޥΤ��s�b�ɮ� | ? �w�״_ |
| �v������ | ���ҥζ}�o�H���Ҧ� | �ҥ�Windows�}�o�H���Ҧ� |
| �����Ĭ� | �ª������M�z | �Ѱ��w���ª��� |
| ñ�p���D | ���վ��ҹL�� | ���s���ʹ��վ��� |

---

## ?? �G�ٱư����n

### �p�G���L�k���p

#### �ˬd1: Windows�}�o�H���Ҧ�
```powershell
# �ˬd�}�o�H���Ҧ����A
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name AllowDevelopmentWithoutDevLicense
```

#### �ˬd2: �M�z�³��p
1. �}��"�]�w" �� "���ε{��"
2. �j�M"GitDWG"
3. �Ѱ��w�˥���{������
4. ���s����

#### �ˬd3: Visual Studio���s���J
1. ����Visual Studio
2. �R��bin�Mobj��Ƨ�
3. ���s�}�ұM��
4. ���ظѨM���

#### �ˬd4: �M��W�ٽĬ�
�T�{Package.appxmanifest����Identity�]�w�G
```xml
<Identity
  Name="GitDWG.CADVersionControl"
  Publisher="CN=GitDWG Development Team"
  Version="1.3.0.0" />
```

---

## ?? ���\���ղM��

����H�U���սT�{�״_���\�G

### �򥻳��p����
- [ ] F5�������`�Ұ�
- [ ] ���ε{���������T���
- [ ] �n�J�\�ॿ�`�u�@
- [ ] �D�n�\��i�H�ϥ�

### �i���\�����
- [ ] Git�x�s�w�ާ@
- [ ] ����ϧκ޲z��
- [ ] �Τ�]�w�t��
- [ ] AutoCAD��X�\��

### ���p�~�����
- [ ] �M��w�˦��\
- [ ] �}�l��涵�إ��T
- [ ] �ϥܥ��`���
- [ ] �Ѱ��w�˰��b����

---

## ?? �p�G�������D

### �����E�_��T
1. Visual Studio��X������������~�T��
2. Windows�ƥ��˵����������ε{�����~
3. ���p�O���ɮ�
4. �t�����Ҹ�T

### �p���䴩�ɴ���
- Windows�����M�իظ��X
- Visual Studio����
- .NET����
- ���㪺���~���|�l��
- ���{�B�J

---

**?? ���ߡIGitDWG�{�b�i�H���`���p�M�����F�I**

**�D�n�״_**: �NPackage.appxmanifest�����ϥܰѷӱq���s�b���۩w�q�ɮקאּ�{�����w�]�ɮסA�ѨM�FMSIX�M��إߥ��Ѫ����D�C