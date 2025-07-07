# GitDWG �ɮ׾ާ@���~�ѨM���n

## ?? �`���ɮ׾ާ@���~�θѨM���

### ���~�GCannot create a file when that file already exists

#### ?? ���~�y�z
�o�ӿ��~�q�`�o�ͦb�O�s�Τ�]�w�ɡA�t�ι��ղ����{���ɮר�ؼЦ�m�A���ؼ��ɮפw�g�s�b�C

#### ?? ���~��]
1. **�ɮ���w**�G�ɮ׳Q��L�{�ǥe��
2. **�v�����D**�G�S���������ɮרt���v��
3. **�õo�ާ@**�G�h�Ӿާ@�P�ɹ��խק�P�@�ɮ�
4. **�ɮרt�ΰ��D**�G�Ϻп��~���ɮרt�ηl�a

#### ? �ѨM���

##### �ߧY�ѨM��k�G
1. **���s�Ұ� GitDWG**
   - �����������ε{��
   - ���� 5-10 ��
   - ���s�}�����ε{��

2. **�ˬd�ɮ��v��**
   ```
   �ɮצ�m�G%LOCALAPPDATA%\GitDWG\user_settings.json
   
   �ˬd�B�J�G
   - �}���ɮ��`��
   - �ɯ��W�z���|
   - �k���I���ɮ� �� ���e �� �w����
   - �T�{�z���Τ�b�ᦳ�u��������v�v��
   ```

3. **�H�޲z����������**
   - �k���I�� GitDWG ���|
   - ��ܡu�H�t�κ޲z����������v

4. **�M�z�{���ɮ�**
   ```powershell
   # �b PowerShell ������
   Remove-Item "$env:LOCALAPPDATA\GitDWG\*.tmp" -Force -ErrorAction SilentlyContinue
   Remove-Item "$env:LOCALAPPDATA\GitDWG\*.backup" -Force -ErrorAction SilentlyContinue
   ```

##### �i���ѨM��k�G

5. **���m�Τ�]�w**
   - �b GitDWG ���I���u�E�_������D�v
   - �d�ݥΤ�]�w�ɮת��A
   - �p�G�����D�A�i�H��ʧR���]�w�ɮ׭��s�}�l

6. **�ˬd�ϺЪŶ�**
   - �T�O�t�κЦ������Ŷ��]�ܤ� 1GB�^
   - �M�z�Ȧs�ɮשM�^����

7. **�״_�ɮרt��**
   ```cmd
   # �b�R�O���ܦr���]�޲z���^������
   chkdsk C: /f /r
   ```

### ���~�GUnauthorizedAccessException

#### ?? ���~�y�z
�S���v���s���]�w�ɮשΥؿ��C

#### ? �ѨM���

1. **�ˬd���r�n��**
   - �N GitDWG �w�˥ؿ��[�J���r�n��զW��
   - �N %LOCALAPPDATA%\GitDWG �[�J�զW��

2. **�״_�ɮ��v��**
   ```powershell
   # �b PowerShell�]�޲z���^������
   $path = "$env:LOCALAPPDATA\GitDWG"
   if (Test-Path $path) {
       icacls $path /grant:r "$env:USERNAME:(OI)(CI)F" /T
   }
   ```

3. **���s�إ߳]�w�ؿ�**
   ```powershell
   # �ƥ��{���]�w�]�p�G�s�b�^
   $settingsPath = "$env:LOCALAPPDATA\GitDWG"
   if (Test-Path $settingsPath) {
       $backupPath = "$settingsPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
       Move-Item $settingsPath $backupPath
   }
   
   # ���s�إߥؿ�
   New-Item -ItemType Directory -Path $settingsPath -Force
   ```

### ���~�GIOException - �ɮ׳Q��L�{�Ǩϥ�

#### ?? ���~�y�z
���վާ@���ɮץ��Q��L�{�ǥe�ΡC

#### ? �ѨM���

1. **�ˬd�����������{��**
   ```powershell
   # �d��e���ɮת��{��
   Get-Process | Where-Object { $_.MainWindowTitle -like "*GitDWG*" }
   ```

2. **�ϥΤu���ˬd�ɮץe��**
   - �U���èϥ� Process Explorer
   - �Ψϥ� Resource Monitor�]�귽�ʵ����^

3. **���s�Ұʹq��**
   - �p�G�L�k���e�ε{�ǡA���s�Ұʹq��

## ?? �w�����I

### �w�����@
1. **�w���M�z�{���ɮ�**
   ```powershell
   # ��ĳ�C�g����@��
   $gitdwgPath = "$env:LOCALAPPDATA\GitDWG"
   Remove-Item "$gitdwgPath\*.tmp" -Force -ErrorAction SilentlyContinue
   Remove-Item "$gitdwgPath\*.backup" -Force -ErrorAction SilentlyContinue
   Remove-Item "$gitdwgPath\*.corrupted_*" -Force -ErrorAction SilentlyContinue
   ```

2. **�ƥ����n�]�w**
   ```powershell
   # �ƥ��Τ�]�w
   $source = "$env:LOCALAPPDATA\GitDWG\user_settings.json"
   $backup = "$env:USERPROFILE\Documents\GitDWG_settings_backup_$(Get-Date -Format 'yyyyMMdd').json"
   if (Test-Path $source) {
       Copy-Item $source $backup
   }
   ```

### �̨ι��
1. **���n�P�ɹB��h�� GitDWG ���**
2. **�w�����s�Ұ����ε{��**
3. **�O���t�Χ�s**
4. **�T�O���r�n�餣�|�z�Z���ε{��**

## ?? �E�_�u��

### �ϥ� GitDWG ���ضE�_
1. �}�� GitDWG
2. �I���u�u��v���u�E�_������D�v
3. �d�ݥΤ�]�w�ɮת��A
4. �ھڶE�_���G�Ĩ��������I

### ����ˬd�M��
```powershell
# ����E�_�}��
$gitdwgPath = "$env:LOCALAPPDATA\GitDWG"
$settingsFile = "$gitdwgPath\user_settings.json"

Write-Host "=== GitDWG �E�_���i ===" -ForegroundColor Green
Write-Host "�ɶ�: $(Get-Date)" -ForegroundColor Gray
Write-Host ""

# �ˬd�ؿ�
Write-Host "�ˬd�ؿ�..." -ForegroundColor Yellow
if (Test-Path $gitdwgPath) {
    Write-Host "? GitDWG �ؿ��s�b: $gitdwgPath" -ForegroundColor Green
    
    # �ˬd�v��
    try {
        $testFile = "$gitdwgPath\test_$(Get-Random).tmp"
        "test" | Out-File $testFile -Encoding UTF8
        Remove-Item $testFile -Force
        Write-Host "? �ؿ��㦳�g�J�v��" -ForegroundColor Green
    } catch {
        Write-Host "? �ؿ��S���g�J�v��: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "? GitDWG �ؿ����s�b" -ForegroundColor Red
}

# �ˬd�]�w�ɮ�
Write-Host "`n�ˬd�]�w�ɮ�..." -ForegroundColor Yellow
if (Test-Path $settingsFile) {
    $fileInfo = Get-Item $settingsFile
    Write-Host "? �]�w�ɮצs�b" -ForegroundColor Green
    Write-Host "   �j�p: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "   �̫�ק�: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    Write-Host "   ��Ū: $($fileInfo.IsReadOnly)" -ForegroundColor Gray
} else {
    Write-Host "?? �]�w�ɮפ��s�b" -ForegroundColor Yellow
}

# �ˬd�{���ɮ�
Write-Host "`n�ˬd�{���ɮ�..." -ForegroundColor Yellow
$tempFiles = Get-ChildItem "$gitdwgPath\*.tmp" -ErrorAction SilentlyContinue
if ($tempFiles) {
    Write-Host "?? �o�{ $($tempFiles.Count) ���{���ɮ�" -ForegroundColor Yellow
    $tempFiles | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Gray }
} else {
    Write-Host "? �S���{���ɮ�" -ForegroundColor Green
}

# �ˬd�ϺЪŶ�
Write-Host "`n�ˬd�ϺЪŶ�..." -ForegroundColor Yellow
$drive = Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DeviceID -eq "C:" }
$freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
if ($freeSpaceGB -gt 1) {
    Write-Host "? �ϺЪŶ��R��: $freeSpaceGB GB" -ForegroundColor Green
} else {
    Write-Host "?? �ϺЪŶ�����: $freeSpaceGB GB" -ForegroundColor Yellow
}

Write-Host "`n=== �E�_���� ===" -ForegroundColor Green
```

## ?? �޳N�䴩

�p�G�W�z��k���L�k�ѨM���D�A���p���޳N�䴩�ô��ѡG

1. **���~�I��**
2. **�E�_���i���G**
3. **Windows ������T**
4. **GitDWG ������T**
5. **�̪񪺾ާ@�B�J**

---

*�̫��s�G2024�~* | *�A�Ϊ����GGitDWG v1.3.0+*