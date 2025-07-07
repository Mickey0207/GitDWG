# ?? GitDWG Windows ���ήM����U���~�״_���n

## ? ���D�w�ѨM

**���~�N�X**: DEP0700, 0x80080204, 0xC00CE169  
**���~�T��**: ���ε{�����U���ѡA���ε{����T�M�����ҿ��~�A�M��W�ټҦ����ŦX�n�D  
**�״_���A**: ? �w����

---

## ?? ���~��]���R

### �ڥ����D
Windows ���ε{���M�� (MSIX) ���M��W�٥����ŦX�S�w���R�W�Ҧ��n�D�G

#### ���ŦX�W�d���W�� (�״_�e)
```xml
<Identity Name="GitDWG.CADVersionControl" />
```

#### �ŦX�W�d���W�� (�״_��)
```xml
<Identity Name="GitDWGDevelopmentTeam.GitDWG" />
```

### Windows �M��W�ٳW�h

#### ? ���Ī��R�W�Ҧ�
- `CompanyName.ProductName`
- `DeveloperName.ApplicationName`
- `PublisherName.AppName`

#### ? �L�Ī��R�W�Ҧ�
- �]�t�S��r�� (���F�I���M�s�r��)
- �H�Ʀr�}�Y
- �]�t�O�d�r��
- �榡���ŦX `{Publisher}.{Product}` �Ҧ�

---

## ??? ����״_�B�J

### 1. **Package.appxmanifest �ץ�**

#### �״_�e�����D�]�w
```xml
<Identity
  Name="GitDWG.CADVersionControl"    <!-- ���D�Ҧb -->
  Publisher="CN=GitDWG Development Team"
  Version="1.3.0.0" />

<mp:PhoneIdentity 
  PhoneProductId="GitDWG.CADVersionControl"  <!-- �P�B�ץ� -->
  PhonePublisherId="00000000-0000-0000-0000-000000000000"/>
```

#### �״_�᪺���T�]�w
```xml
<Identity
  Name="GitDWGDevelopmentTeam.GitDWG"    <!-- �ŦX�W�d -->
  Publisher="CN=GitDWG Development Team"
  Version="1.3.0.0" />

<mp:PhoneIdentity 
  PhoneProductId="GitDWGDevelopmentTeam.GitDWG"  <!-- �O���@�P -->
  PhonePublisherId="00000000-0000-0000-0000-000000000000"/>
```

### 2. **�R�W�W�h�Ը�**

#### �M��W�ٲզ�
```
�榡: {�o��̼���}.{���~�W��}
�d��: GitDWGDevelopmentTeam.GitDWG
����: �u�w �o��̦W�� (�h���Ů�M�S��r��)
      �|�w ���~�W�� (²����T)
```

#### ���Ħr�Ŷ�
- **���\**: �r�� (a-z, A-Z)
- **���\**: �Ʀr (0-9)  
- **���\**: �I�� (.)
- **���\**: �s�r�� (-)
- **�T��**: �Ů�B�S��Ÿ��BUnicode�r��

### 3. **�ظm����**
? �M�׫ظm���\�A�L���~

---

## ?? �������~�N�X����

### DEP0700 ���~
- **����**: ���p���~
- **��]**: ���ε{���M����U����
- **�v�T**: �L�k�w�˩ΰ������ε{��

### 0x80080204 ���~
- **����**: COM���~
- **��]**: ���ε{����T�M�����ҥ���
- **�Բ�**: �M��M��榡�Τ��e���ŦX�n�D

### 0xC00CE169 ���~
- **����**: XML���ҿ��~
- **��]**: �M��W�ټҦ����ҥ���
- **�W�h**: ���ŦX Windows �M��R�W�W�d

---

## ?? �G�ٱư����n

### �p�G���M�J���������D

#### �ˬd1: �M��W�ٮ榡
```xml
<!-- �T�{�榡���T -->
<Identity Name="PublisherName.AppName" />
```

#### �ˬd2: �r�Ŧ��ĩ�
- �T�O�u�ϥΦr���B�Ʀr�B�I���B�s�r��
- �קK�Ů�M�S��r��
- ���n�H�Ʀr�}�Y

#### �ˬd3: ���׭���
- �M��W���`���פ��W�L50�r��
- �U�������צX�z

#### �ˬd4: �O�d�r��
�קK�ϥ�Windows�O�d�r���G
- `Windows`, `Microsoft`, `System` ��

### �M�z�B�J (�p�G�ݭn)

#### 1. �M�z�³��p
```powershell
# �Ѱ��w�˥���{������
Get-AppxPackage *GitDWG* | Remove-AppxPackage
```

#### 2. �M�zVisual Studio�֨�
```
1. ���� Visual Studio
2. �R�� bin �M obj ��Ƨ�
3. �M�z %TEMP% �����ظm�֨�
4. ���s�}�ұM��
```

#### 3. ���s�ظm
```
1. �M�z�ѨM���
2. ���ظѨM���
3. ���ճ��p
```

---

## ?? �̨ι���ĳ

### �M��R�W�̨ι��

#### 1. �ϥΤ��q/�}�o�̦W��
```xml
<!-- ���ˮ榡 -->
<Identity Name="CompanyName.ProductName" />
<Identity Name="DeveloperName.AppName" />
```

#### 2. �O��²����T
```xml
<!-- �n���d�� -->
<Identity Name="GitDWGTeam.GitDWG" />
<Identity Name="AcmeInc.CADTool" />

<!-- �קK���d�� -->
<Identity Name="My.Company.Super.Long.Product.Name" />
<Identity Name="Special@Characters#Not$Allowed" />
```

#### 3. �����@�P��
�T�O�Ҧ��������O���@�P�G
```xml
<Identity Name="GitDWGDevelopmentTeam.GitDWG" />
<mp:PhoneIdentity PhoneProductId="GitDWGDevelopmentTeam.GitDWG" />
```

### ���p���լy�{

#### 1. ��������
- F5 ��������
- �ˬd�ҰʩM�򥻥\��

#### 2. �M�����
- �إ�MSIX�M��
- ���զw�˩M�Ѱ��w��
- ���Ҷ}�l��涵��

#### 3. �ɯŴ���
- ���ժ����ɯŬy�{
- �T�{�]�w�M��ƫO�d

---

## ?? �״_�ĪG����

### ? ���\����

#### �ظm���q
- [x] �L�sĶ���~
- [x] �L�s�����~
- [x] �M��M�����ҳq�L

#### ���p���q
- [x] ���ε{�����U���\
- [x] �M��w�˦��\
- [x] �}�l��涵�إ��T���

#### �B�涥�q
- [x] ���ε{�����`�Ұ�
- [x] �\�ॿ�`�B�@
- [x] �L�B��ɿ��~

### ?? ��ĳ����

#### 1. �򥻥\�����
```
�� ���ε{���Ұ�
�� �Τ�n�J�t��
�� Git�x�s�w�ާ@
�� ����ϧκ޲z��
�� CAD�ɮ׳B�z
```

#### 2. �M��޲z����
```
�� �����w��
�� �����ɯ�
�� ����Ѱ��w��
�� ���s�w��
```

---

## ?? �w�����I

### �}�o���q
1. **�R�W�W��**: �b�M�ת���T�w�ŦX�W�d���M��W��
2. **�榡����**: �w���ˬdPackage.appxmanifest���榡���T��
3. **���ճ��p**: �W�c�i�楻�����p����

### ���@���q
1. **�����޲z**: �T�O���������W�M�榡���T
2. **�M���s**: �ԷV�ק�M��M�椺�e
3. **�ۮe��**: �Ҽ{�P�ª������ɯŬۮe��

---

## ?? ��L�`�����D

### Q: �ק�M��W�٫�|�v�T�{���Τ�ܡH
**A**: �O���A�M��W���ܧ�|�Q�����s���ε{���C��ĳ�G
- ���Ѳ���u��
- �O�d�Τ�]�w�M���
- ���Ѥɯū���

### Q: �i�H�ϥΤ���r�ŧ@���M��W�ٶܡH
**A**: ����ĳ�CWindows�M��W�����Өϥ�ASCII�r�ťH�T�O�̤j�ۮe�ʡC

### Q: �p���ܦX�A��Publisher�W�١H
**A**: �ϥΤ��q�ζ}�o�̪��з��ѧO�W�١A�קK�S��r�ũM�Ů�C

---

## ?? �״_�����`��

### ? ���D�ѨM���A
- **�M��W��**: GitDWG.CADVersionControl �� GitDWGDevelopmentTeam.GitDWG
- **�榡����**: ? �ŦXWindows�R�W�W�d
- **�ظm���A**: ? ���\
- **���p�ǳ�**: ? �N��

### ?? �ﵽ�ĪG
- �������p���~
- �����M��w�˦��\�v
- �ŦXWindows Store�W�d
- ���ɱM�~�ζH

**?? GitDWG �{�b��ƲŦX Windows �W�d���M��W�١A�i�H���Q�i�泡�p�M�o�G�I**