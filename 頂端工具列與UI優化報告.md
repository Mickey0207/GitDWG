# GitDWG ���ݤu��C�PUI�u�Ƴ��i

## ?? ��i�ؼЧ���

�ھڱz���ݨD�A�ڭ̤w�g���\��{�F�H�U��ӥD�n�ؼСG
1. ? **�W�[���ݥi���|�u��C** - �N�ۦ��\����ըä䴩�۰ʮi�}
2. ? **�����Ҧ����Ÿ�** - ���ѧ�M�~���ɭ��~�[

## ?? ���ݤu��C�]�p

### �\����լ[�c
�ڭ̱N�쥻���ê��u����s���s��´���T���޿���աG

#### 1. Git�ާ@��
```
����x�s�w | ��l���x�s�w | ���s��z | �j��s��z
```
- **�γ~**: ��¦Git�x�s�w�޲z
- **��m**: �u��C�Ĥ@��
- **�S��**: �̱`�Ϊ�Git�ާ@�����b�@�_

#### 2. CAD�u���
```
�ˬdCAD�ɮ� | �]�wAutoCAD���| | �E�_������D
```
- **�γ~**: AutoCAD�����\��
- **��m**: �u��C�ĤG��
- **�S��**: CAD�M�Τu��W�ߤ���

#### 3. �t�γ]�w��
```
�Τ�]�w | [�ثe�������]
```
- **�γ~**: �t�ΰt�m�M���A���
- **��m**: �u��C�ĤT��
- **�S��**: �]�t�����T����ı�����

### �۰ʮi�}����

#### ���|���A (�w�])
- **����**: 32px
- **��ܤ��e**: "�u��C (�ƹ��a���i�})" + �����T
- **��ı**: ²�䪺���ܦC�A�����ιL�h�Ŷ�

#### �i�}���A (�ƹ��a��)
- **Ĳ�o**: �ƹ����J����32px�ϰ�
- **���e**: ���㪺�T�h�\����s
- **����**: �ƹ��b�u��C�ϰ줺�ɫO���i�}
- **����**: �ƹ����}��300ms�۰ʦ���

#### ���z����
```csharp
// �i�}�޿�
�ƹ��i�JĲ�o�ϰ� �� ��������p�ɾ� �� �ߧY�i�}�u��C

// �����޿�  
�ƹ����}�u��C �� �Ұ�300ms�p�ɾ� �� �۰ʦ���
```

## ?? ���Ÿ��M�z

### �M�z�d��
�ڭ̨t�Ωʦa�����F�ɭ������Ҧ����Ÿ��G

#### �D���f (MainWindow.xaml)
- ? ����: ?? (�ֳt������s)
- ? ����: ?? (������r�}�Y)
- ? ���G: �¤�r���M�~���s�M����

#### �n�J���f (UserLoginWindow.cs)
| ��l��r | �M�z���r | ��m |
|----------|------------|------|
| ?? GitDWG �Τ�n�J | GitDWG �Τ�n�J | ���D |
| ��ܥΤ� ?? | ��ܥΤ� | �Τ��ܼ��� |
| ��J�K�X ?? | ��J�K�X | �K�X��J���� |
| ?? �n�J | �n�J | �n�J���s |
| ? �s�W�Τ� | �s�W�Τ� | �s�W�Τ���s |
| ? �h�X���ε{�� | �h�X���ε{�� | �h�X���s |
| ?? �ϥλ��� | �ϥλ��� | �����ϰ���D |
| �޲z���K�X ?? | �޲z���K�X | ��ܮ���� |
| �s�Τ�W�� ?? | �s�Τ�W�� | ��ܮ���� |
| �s�Τ�K�X ?? | �s�Τ�K�X | ��ܮ���� |
| ? ���~�T�� | ���~: �T�� | ���~���� |
| ? ���\�T�� | ���\: �T�� | ���\���� |

### �M�~�ƮĪG
- **�@�P��**: �Ҧ��ɭ������ϥί¤�r
- **�iŪ��**: ������ı�z�Z�A������r�M����  
- **�M�~�P**: �ŦX���~�����Ϊ���ı�з�
- **��ڤ�**: �K�󥼨Ӧh�y���䴩

## ??? �޳N��{�Ӹ`

### XAML���c�]�p
```xml
<Grid x:Name="TopToolbarArea">
    <!-- ���骬�A -->
    <Border x:Name="CollapsedToolbar" Height="32">
        <Grid>
            <TextBlock Text="�u��C (�ƹ��a���i�})"/>
            <StackPanel><!-- �����T --></StackPanel>
        </Grid>
    </Border>
    
    <!-- �i�}���A -->
    <Border x:Name="ExpandedToolbar" Visibility="Collapsed">
        <Grid>
            <StackPanel Grid.Row="0"><!-- Git�ާ@ --></StackPanel>
            <StackPanel Grid.Row="1"><!-- CAD�u�� --></StackPanel>
            <StackPanel Grid.Row="2"><!-- �t�γ]�w --></StackPanel>
        </Grid>
    </Border>
    
    <!-- Ĳ�o�ϰ� -->
    <Border x:Name="HoverTrigger" Background="Transparent"/>
</Grid>
```

### C# �ƥ�B�z
```csharp
public sealed partial class MainWindow : Window
{
    private DispatcherTimer _collapseTimer;
    private bool _isToolbarExpanded = false;

    // �ƹ��i�J�ƥ�
    private void HoverTrigger_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _collapseTimer.Stop();
        if (!_isToolbarExpanded) ExpandToolbar();
    }

    // �ƹ����}�ƥ�  
    private void HoverTrigger_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _collapseTimer.Start(); // 300ms���𦬧�
    }
}
```

### ���A�޲z
- **�i�}���A**: `_isToolbarExpanded = true`
- **���骬�A**: `_isToolbarExpanded = false`  
- **�p�ɱ���**: 300ms `DispatcherTimer`
- **��ı����**: `Visibility.Collapsed` / `Visibility.Visible`

## ?? �Τ�����ﵽ

### �Ŷ��Q���u��
| ���A | ���� | ���e | �u�� |
|------|------|------|------|
| ���� | 32px | ����+���� | �`�٪Ŷ��A�O���D�n���e�ϰ� |
| �i�} | ~120px | ����u��C | �ֳt�s���Ҧ��\�� |

### �ާ@�Ĳv����
1. **�ֳt�s��**: �ƹ��a���Y�i�i�}�A�L���I��
2. **�\�����**: �����\��E���A��ֵ�ı�j�M�ɶ�
3. **���A�O��**: �ާ@�L�{���u��C�O���i�}
4. **�۰ʦ���**: ���}��۰ʦ���A�O���ɭ����

### ��ı�h���ﵽ
- **�D������**: �`�Υ\���u�����
- **���ղM��**: �T�ӥ\��յ�ı����
- **���A���T**: �����T��X���
- **�M�~�~�[**: �������Ÿ���󥿦�

## ?? �ϥΫ��n

### �u��C�ާ@
1. **�i�}�u��C**: �N�ƹ����ʨ��������
2. **�O���i�}**: �b�u��C�ϰ줺���ʷƹ�
3. **�ϥΥ\��**: �I���������\����s
4. **�۰ʦ���**: �ƹ����}�u��C�ϰ�

### �\��d��
- **Git�ާ@**: �i�}��Ĥ@��
- **CAD�u��**: �i�}��ĤG��  
- **�t�γ]�w**: �i�}��ĤT��
- **�����T**: �l����ܦb�k��

## ?? �����X�i��

### �u��C�w���
- **���s����**: �i�t�m���s�ƦC����
- **���ճ]�w**: �䴩�۩w�q�\�����
- **�ֱ���**: ���`�Υ\��K�[��L�ֱ���
- **�D�D�A�t**: �u��C�˦����H�t�ΥD�D

### ���z�ƯS��
- **�ϥ��W�v**: �ھڨϥ��W�v�վ���s����
- **�W�U��P��**: �ھڷ�e���A��ܬ����u��
- **�ֳt�j�M**: �b�u��C���K�[�\��j�M
- **�ϥδ���**: �s��޾ɩM�\�໡��

## ? ���G�`��

### �D�n���N
? **�Ŷ��Ĳv**: ���骬�A�Ȧ���32px����  
? **�\�৹��**: �Ҧ��즳�\�৹��O�d  
? **�ާ@�K�Q**: �ƹ��a���Y�i�ֳt�s��  
? **�M�~�~�[**: �������Ÿ��A�󥿦�  
? **�����޿�**: ���\�������X�z����  

### �Τ���q
- **��j�u�@�ϰ�**: �w�]���A�U�`�٫����Ŷ�
- **��־ާ@**: ����I�����ơA�����Ĳv
- **��M���ɭ�**: �M�~�ƥ~�[�A��ֵ�ı�z�Z
- **��n��´**: �\��������d���e��

---

**�ɯŪ��A**: ? �w����  
**�v�T�d��**: ?? �D���f�M�n�J���f  
**���紣��**: ?? ��ۧﵽ  
**�M�~��**: ?? ���~�żз�

�{�bGitDWG�֦��{�N�ƪ��i���|�u��C�M�M�~���L���Ÿ��ɭ��I