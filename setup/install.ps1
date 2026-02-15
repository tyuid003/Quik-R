$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$publishPath = Join-Path $projectRoot 'bin\Release\net8.0-windows\win-x64\publish'
$sourceExe = Join-Path $publishPath 'QuickReply.exe'

if (-not (Test-Path $sourceExe)) {
    throw "ไม่พบไฟล์ $sourceExe กรุณา build publish ก่อน"
}

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Quik-R'
New-Item -ItemType Directory -Path $installDir -Force | Out-Null

Copy-Item -Path (Join-Path $publishPath '*') -Destination $installDir -Recurse -Force

$desktopShortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Quik-R.lnk'
$startMenuDir = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\Quik-R'
$startMenuShortcutPath = Join-Path $startMenuDir 'Quik-R.lnk'
New-Item -ItemType Directory -Path $startMenuDir -Force | Out-Null

$wshell = New-Object -ComObject WScript.Shell

$desktopShortcut = $wshell.CreateShortcut($desktopShortcutPath)
$desktopShortcut.TargetPath = Join-Path $installDir 'QuickReply.exe'
$desktopShortcut.WorkingDirectory = $installDir
$desktopShortcut.IconLocation = Join-Path $installDir 'QuickReply.exe'
$desktopShortcut.Save()

$startMenuShortcut = $wshell.CreateShortcut($startMenuShortcutPath)
$startMenuShortcut.TargetPath = Join-Path $installDir 'QuickReply.exe'
$startMenuShortcut.WorkingDirectory = $installDir
$startMenuShortcut.IconLocation = Join-Path $installDir 'QuickReply.exe'
$startMenuShortcut.Save()

Write-Host "ติดตั้งเสร็จแล้ว: $installDir"
Write-Host "เปิดโปรแกรมได้จาก Desktop หรือ Start Menu (Quik-R)"
