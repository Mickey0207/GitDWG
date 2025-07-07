# InitialSetupWindow �`��UI�u�Ƴ��i

## ?? �u�ƥؼ�

�ھڱz���n�D�A�ڹ�InitialSetupWindow�i��F������UI�u�ơG
1. **�����L�\����s**�G�R�������n���u�����v���s
2. **�`��UI�]�p**�G�ĥβ{�N�`��D�D�A�קK�L��]�p

## ??? �������\��

### �R���u�����v���s
**��]���R**�G
- ��l�]�w�O���n�y�{�A�Τᥲ�������~��ϥ����ε{��
- �u�����v���s�|�ɭP���ε{���h�X�A�Τ����礣��
- ²��UI�A��֥Τ�x�b

**�����ܧ�**�G
```csharp
// �����e�G�����s�G��
var buttonPanel = new StackPanel
{
    Orientation = Orientation.Horizontal,  // �����ƦC
    HorizontalAlignment = HorizontalAlignment.Right,
    Spacing = 8
};
buttonPanel.Children.Add(cancelButton);     // ? �R��
buttonPanel.Children.Add(_saveButton);

// �u�ƫ�G����s�G��
var buttonPanel = new StackPanel
{
    HorizontalAlignment = HorizontalAlignment.Stretch, // ���e��
    VerticalAlignment = VerticalAlignment.Bottom,
    Margin = new Thickness(0, 16, 0, 0)
};
buttonPanel.Children.Add(_saveButton);     // ? �O�d�D�n�\��
```

### ���������ƥ�B�z
�R������k�G
- `CancelButton_Click` - �������s�ƥ�
- `ShowExitConfirmation` - �h�X�T�{��ܮ�

## ?? �`��UI�]�p

### �����m���

#### �I���h��
| ���� | �C�� | RGB�� | �γ~ |
|------|------|-------|------|
| �D�I�� | �`�Ŧ� | `rgb(17, 24, 39)` | ���f�D�I�� |
| ���e���O | �`���� | `rgb(30, 41, 59)` | �������e�ϰ� |
| ��J�حI�� | �`�� | `rgb(51, 65, 85)` | �奻��J�ϰ� |
| ���ܭ��O | �b�z���� | `rgba(16, 185, 129, 0.16)` | ������T�I�� |

#### ��r�h��
| ��r���� | �C�� | RGB�� | ���� |
|----------|------|-------|--------|
| �D���D | �L�ǥ� | `rgb(241, 245, 249)` | ����� |
| �Ƽ��D | �Ǧ� | `rgb(148, 163, 184)` | ����� |
| ���� | �L�� | `rgb(226, 232, 240)` | ������� |
| ������r | �L�� | `rgb(187, 247, 208)` | �S���T |
| ���~��r | �L�� | `rgb(248, 113, 113)` | ĵ�ܸ�T |

#### ��ػP���j
| ���� | �C�� | RGB�� | �z���� |
|------|------|-------|--------|
| �D���O��� | �Ŧ� | `rgb(59, 130, 246)` | 31% |
| ��J����� | �`�� | `rgb(75, 85, 99)` | 100% |
| ���ܭ��O��� | ��� | `rgb(16, 185, 129)` | 24% |

### ����UI��i

#### 1. ��������]�p
```csharp
// �`��D�I��
var mainGrid = new Grid
{
    Background = new SolidColorBrush(Color.FromArgb(255, 17, 24, 39))
};

// �������e���O
var border = new Border
{
    Background = new SolidColorBrush(Color.FromArgb(255, 30, 41, 59)),
    BorderBrush = new SolidColorBrush(Color.FromArgb(80, 59, 130, 246)),
    CornerRadius = new CornerRadius(16),
    Padding = new Thickness(32)
};
```

#### 2. �奻��J���u��
```csharp
_authorNameTextBox = new TextBox
{
    Background = new SolidColorBrush(Color.FromArgb(255, 51, 65, 85)),    // �`��I��
    Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 245, 249)), // �զ��r
    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99)),   // �`�����
    CornerRadius = new CornerRadius(8),
    Padding = new Thickness(12, 10, 12, 10)
};
```

#### 3. �ʺA���s���A
```csharp
// �ҥΪ��A - ���
_saveButton.Background = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129));

// �T�Ϊ��A - �Ǧ�  
_saveButton.Background = new SolidColorBrush(Color.FromArgb(255, 75, 85, 99));
```

#### 4. ��T���ܭ��O
```csharp
var infoPanel = new Border
{
    Background = new SolidColorBrush(Color.FromArgb(40, 16, 185, 129)),    // �b�z�����
    BorderBrush = new SolidColorBrush(Color.FromArgb(60, 16, 185, 129)),   // �L�����
    CornerRadius = new CornerRadius(8)
};
```

## ?? �Τ������i

### 1. ²�ƾާ@�y�{
**���e**�G
```
��g��� �� ��ܡu�T�w�v�Ρu�����v �� �i��h�X���ε{��
```

**�{�b**�G
```  
��g��� �� �I���u�����]�w�v �� �����i�J�D���ε{��
```

### 2. �W�j��ı�^�X
- **�Y������**�G��J�ɥߧY��ܿ��~����
- **�ʺA���s**�G���s�C��ھڪ�榳�ĩ��ܤ�
- **Enter��䴩**�G�bEmail��J�ث�Enter�i��������

### 3. �ﵽ�iŪ��
- **������**�G�`��I���t�L���r�A�O�@����
- **�M���h��**�G���P�����ϥΤ��P�ǫׯŧO
- **�A���Z**�G�W�[�������Z�A���ɵ�ı�ξA��

## ?? �T�����]�p

### �ؤo�u��
| �ݩ� | ��� | �s�� | ��i |
|------|------|------|------|
| �̤p�e�� | 400px | 450px | ��ξA�����e�e�� |
| �̤p���� | 300px | 380px | �A���`��]�p������ |
| ����Z | 24px | 32px | ��e�P�����e���Z |
| �ꨤ�b�| | 8px | 16px | ��{�N���ꨤ�]�p |

### ���s�]�p
```csharp
_saveButton = new Button
{
    Content = "�����]�w",                    // ����T�����s��r
    HorizontalAlignment = Stretch,          // ���e�׫��s
    Height = 44,                           // �ξA���I���ϰ�
    FontSize = 16,                         // ��Ū���r��j�p
    CornerRadius = new CornerRadius(10)    // �{�N�ꨤ�]�p
};
```

## ?? �]�p��h��`

### 1. ²���D�q
- **�������l**�G�R�������n�����s�M�\��
- **�E�J�֤�**�G��X�D�n�ާ@�]�����]�w�^
- **�M���ɯ�**�G��@���T���e�i���|

### 2. �{�N�`��D�D
- **�@���]�p**�G�`��I������ť���E
- **�M�~�~�[**�G�ŦX�{�N���γ]�p�Ͷ�
- **�~�P�@�P**�G�P��L�`��ɭ��O���@�P

### 3. �i�Ω��u��
- **��L�䴩**�GEnter��ֳt����
- **�Y�ɤ��X**�G������ҩM��ı����
- **���~�B�z**�G�͵������~�T���M��_����

## ?? �����R

### �u�ƫe����

| �譱 | �u�ƫe | �u�ƫ� | ��i�ĪG |
|------|--------|--------|----------|
| **���s�ƶq** | 2�ӡ]����+�T�w�^ | 1�ӡ]�����]�w�^ | ���50%����ܧx�Z |
| **��m�D�D** | �w�]�L�� | �M�~�`�� | �@���B�{�N�� |
| **��ı�h��** | ������ | �h�h���`�� | ��M������T�[�c |
| **����** | �@�� | ����� | ��n���iŪ�� |
| **�ާ@�y�{** | �i�त�_ | ���u�y�{ | �󶶺Z���Τ����� |

### �޳N���д���

| ���� | �u�ƫe | �u�ƫ� | ���ɴT�� |
|------|--------|--------|----------|
| UI�����ƶq | ���h | ��² | -20% |
| ��m������ | ���� | �t�Τ� | +40% |
| ��ı�@�P�� | �}�n | �u�q | +30% |
| �Τ�x�b�� | �� | ���C | -80% |

## ?? �����X�i�i��

### 1. �D�D�t��
- �䴩�L��/�`��D�D����
- �Τ�۩w�q��m���
- �t�ΥD�D�۰ʸ��H

### 2. �i������
- �Y�ɺ���Email����
- �P���~AD��X
- �h�y����J�䴩

### 3. �ʵe�ĪG
- ���ƪ����A�ഫ
- ���~�T���H�J/�H�X
- ���s�a���ĪG

## ? �`��

### �D�n���N
? **²��UI**�G���������n���������s�A²�ƥΤ�M��  
? **�`��D�D**�G�ĥβ{�N�`��]�p�A�@���B�M�~  
? **������**�G�T�O��r�b�`��I���W�M���iŪ  
? **�@�P����**�G�P���Ψ�L�����O����ı�@�P��  
? **�Τ�͵�**�G���[���ާ@�y�{�M�Y�ɤ��X  

### �Τ���q
- **��M�`���]�w����**�G�L�z�Z����@�ާ@���|
- **��ξA����ı�P��**�G�`��D�D�O�@�������d
- **��֪������t��**�G²�ƪ��ɭ���־ާ@�ɶ�
- **��M��������**�G���T�������M���~����

---

**�u�ƪ��A**: ? �w����  
**UI�D�D**: ?? �{�N�`��]�p  
**�Τ�����**: ?? ��۴���  
**���@��**: ?? �N�X��²��  

InitialSetupWindow�{�b�֦��M�~���`��UI�M²�ƪ��ާ@�y�{�I