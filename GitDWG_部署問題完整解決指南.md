# ?? GitDWG ���p���D����E�_�M�ѨM���n

## ?? ��e�E�_���A

**�M�ת��A**: ? �ظm���\  
**Package.appxmanifest**: ? �榡���T  
**Assets�ɮ�**: ? �����s�b  
**�}�o�H���Ҧ�**: ? �w�ҥ�  
**�{���M��**: ? �L�Ĭ�  

---

## ?? ���p���D�����E�_

### 1. **�ߧY�ˬd - Configuration Manager�]�w**

#### Visual Studio �]�w�ˬd
1. �}�� Visual Studio
2. ��ܿ��G**�ظm** �� **Configuration Manager**
3. �T�{�H�U�]�w�G

| �]�w���� | ��ĳ�� | �ˬd���A |
|----------|--------|----------|
| **�M�צW��** | GitDWG | �� |
| **�]�w** | Debug (�� Release) | �� |
| **���x** | x64 (����) | �� |
| **�ظm** | ? �w�Ŀ� | �� |
| **���p** | ? �w�Ŀ� | ?? **����** |

#### �p�G"���p"���Ŀ�
1. �bConfiguration Manager���Ŀ�GitDWG�M�ת�"���p"�֨����
2. �I��"�T�w"
3. ���s����F5����

### 2. **�`�����p���~�����M�ѨM���**

#### A. DEP0700 - ���ε{�����U����
**�g��**: �M����U����
**�ѨM���**:
```powershell
# �M�z�®M��
Get-AppxPackage *GitDWG* | Remove-AppxPackage -ErrorAction SilentlyContinue

# �M�zVisual Studio�֨�
# �R�� bin �M obj ��Ƨ�
```

#### B. 0x80073CF6 - �M��w�w�˥B������
**�g��**: �L�k�w�˸��C����
**�ѨM���**:
```xml
<!-- �bPackage.appxmanifest���W�[������ -->
<Identity Version="1.3.1.0" />
```

#### C. 0x80073D06 - �M��ñ�����D
**�g��**: ñ�����ҥ���
**�ѨM���**:
1. �T�O�ϥ�Debug�]�w�i�氻��
2. �ˬd�}�o�H���Ҧ��w�ҥ�

#### D. 0x80073CF9 - �M���s����
**�g��**: �L�k��s�{���M��
**�ѨM���**:
```powershell
# ���������{���M��
Get-AppxPackage *GitDWGDevelopmentTeam* | Remove-AppxPackage
```

### 3. **�Բӳ��p�ˬd�M��**

#### �B�J1: �����ˬd
```powershell
# �ˬdWindows����
Get-ComputerInfo | Select WindowsProductName, WindowsVersion

# �ˬd.NET 8�B���
dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 8"

# �ˬd�}�o�H���Ҧ�
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" -Name AllowDevelopmentWithoutDevLicense
```

#### �B�J2: �M���ˬd
```
�� GitDWG.csproj �]�w���T
�� Package.appxmanifest �榡����
�� Assets ��Ƨ��]�t�Ҧ����n�ϥ�
�� �ؼЮج[�� net8.0-windows
�� �M�׫ظm�L���~
```

#### �B�J3: Visual Studio�ˬd
```
�� �ҰʱM�׳]�w�� GitDWG
�� Configuration Manager ��"���p"�w�Ŀ�
�� ���x�]�w���T (x64����)
�� �L�sĶĵ�i�ο��~
```

### 4. **�v�B���p����**

#### ����1: �򥻫ظm
```
1. �M�z�ѨM��� (Build �� Clean Solution)
2. ���ظѨM��� (Build �� Rebuild Solution)
3. �ˬd��X�����L���~
```

#### ����2: ���p�ǳ�
```
1. �T�{Configuration Manager�]�w
2. �ˬd�M��M��L�Ĭ�
3. ���ҩҦ��귽�ɮצs�b
```

#### ����3: ��ڳ��p
```
1. ��F5�}�l����
2. �[���X�������p�T��
3. �ˬd���ε{���O�_���`�Ұ�
```

---

## ??? ����ѨM�B�J

### ���1: �зǳ��p�״_
```
1. ���� Visual Studio
2. �R���M�ץؿ��U�� bin �M obj ��Ƨ�
3. �}�� Visual Studio
4. ���J�M��
5. �T�{ Configuration Manager �]�w
6. ���ظѨM���
7. �� F5 �}�l����
```

### ���2: �`�ײM�z�״_
```powershell
# PowerShell ���� (�H�޲z������)

# 1. �����Ҧ������M��
Get-AppxPackage *GitDWG* | Remove-AppxPackage -ErrorAction SilentlyContinue

# 2. �M�z�M��֨�
Remove-Item "$env:LOCALAPPDATA\Packages\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue

# 3. �M�z�Ȧs�ɮ�
Remove-Item "$env:TEMP\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue
```

### ���3: �}�o�H���Ҧ����m
```
1. �}�ҳ]�w �� ��s�P�w���� �� �}�o�H���ﶵ
2. �����}�o�H���Ҧ�
3. ���s�Ұʹq��
4. ���s�ҥζ}�o�H���Ҧ�
5. ���ճ��p
```

### ���4: �M��W���ˬd
�T�{Package.appxmanifest�����W�ٮ榡���T�G
```xml
<Identity
  Name="GitDWGDevelopmentTeam.GitDWG"    <!-- ���T�榡 -->
  Publisher="CN=GitDWG Development Team"
  Version="1.3.0.0" />
```

---

## ?? �G�ٱư��u��

### Visual Studio�E�_
1. **��X����**: �d�ݫظm�M���p�T��
2. **���~�M��**: �ˬd�sĶ���~�Mĵ�i
3. **������X**: �d�ݹB��ɰT��

### PowerShell�E�_�R�O
```powershell
# �ˬd�M�󪬺A
Get-AppxPackage | Where-Object {$_.Name -like "*GitDWG*"}

# �ˬd���p�O��
Get-WinEvent -LogName "Microsoft-Windows-AppxDeployment-Server/Operational" -MaxEvents 10

# �ˬd�}�o�H���]�w
Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
```

### �t�Ψƥ��ˬd
1. �}�� **�ƥ��˵���**
2. �ɯ��G**���ε{���ΪA�ȰO��** �� **Microsoft** �� **Windows** �� **AppxDeployment-Server**
3. �d�ݳ̷s�����~��ĵ�i�ƥ�

---

## ?? ���~�N�X��Ӫ�

| ���~�N�X | ���� | �ѨM��� |
|----------|------|----------|
| **DEP0700** | ���ε{�����U���� | �ˬd�M��M��B�M�z�®M�� |
| **0x80073CF6** | �����Ĭ� | �W�[�������β����ª��� |
| **0x80073D06** | ñ�����D | �ˬd�}�o�H���Ҧ� |
| **0x80073CF9** | ��s���� | ���������᭫�s�w�� |
| **0x80073D02** | �M��l�a | ���s�ظm�M�� |
| **0x80070002** | �ɮץ���� | �ˬdAssets�ɮ� |

---

## ?? ���p���\���ҲM��

### ���p���q�ˬd
```
�� Visual Studio��X���"���p���\"
�� �L���~��ĵ�i�T��
�� �M��w�w�˨�t��
�� �}�l���X�{���ε{���ϥ�
```

### �B�涥�q�ˬd
```
�� ���ε{�����`�Ұ�
�� �n�J�������T���
�� �D�n�\�ॿ�`�B�@
�� �L�B��ɿ��~
```

### �\������
```
�� �Τ�n�J�t��
�� Git�x�s�w�ާ@
�� ����ϧκ޲z��
�� CAD�ɮ׳B�z
�� �]�w�O�s�M���J
```

---

## ?? �ֳt�״_�R�O��

### �@��M�z�}��
```powershell
# �إߨð��榹PowerShell�}�� (�H�޲z������)
Write-Host "�M�zGitDWG���p����..." -ForegroundColor Green

# �����{���M��
Get-AppxPackage *GitDWG* | Remove-AppxPackage -ErrorAction SilentlyContinue
Write-Host "? �w�M�z�{���M��" -ForegroundColor Green

# �M�z�֨�
Remove-Item "$env:LOCALAPPDATA\Packages\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "? �w�M�z�M��֨�" -ForegroundColor Green

# �M�z�Ȧs�ɮ�
Remove-Item "$env:TEMP\*GitDWG*" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "? �w�M�z�Ȧs�ɮ�" -ForegroundColor Green

Write-Host "�M�z�����I�Э��s�Ұ�Visual Studio�ù��ճ��p�C" -ForegroundColor Yellow
```

### Visual Studio���m�B�J
```
1. �����Ҧ�Visual Studio���
2. ����W�z�M�z�}��
3. ���s�Ұʹq�� (�i��A����ĳ)
4. �}��Visual Studio
5. ���JGitDWG�M��
6. �ˬdConfiguration Manager�]�w
7. ��F5���ճ��p
```

---

## ?? �p�G���D����s�b

### �����E�_��T
```
1. Visual Studio�����M�իظ��X
2. Windows�����M�իظ��X
3. ���㪺���~�T���M���|�l��
4. �ƥ��˵��������������~
5. Package.appxmanifest�����㤺�e
```

### �i���E�_
```
1. ���ճЫطs���ť�WinUI 3�M�רó��p
2. �ˬdVisual Studio�w�ˬO�_����
3. �T�{Windows SDK�����ۮe��
4. ���եH�޲z����������Visual Studio
```

---

## ?? �̫ᴣ��

### �����T�{�������I
1. ? **Configuration Manager��"���p"�w�Ŀ�** - �o�O�̱`�������D
2. ? **�}�o�H���Ҧ��w�ҥ�**
3. ? **�L�{���M��Ĭ�**
4. ? **Package.appxmanifest�榡���T**
5. ? **�Ҧ�Assets�ɮצs�b**

### ���˪����p����
```
���ˬd �� �M�z���� �� ���رM�� �� �ˬd�]�w �� ���p����
```

**�p�G��`�����n�ᤴ�L�k���p�A�д��Ѩ��骺���~�T���H�K�i�@�B��U�I** ??