$ErrorActionPreference = 'Stop'

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Quik-R'
$desktopShortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Quik-R.lnk'
$startMenuDir = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\Quik-R'

Get-Process QuickReply -ErrorAction SilentlyContinue | Stop-Process -Force

if (Test-Path $desktopShortcutPath) {
    Remove-Item $desktopShortcutPath -Force
}

if (Test-Path $startMenuDir) {
    Remove-Item $startMenuDir -Recurse -Force
}

if (Test-Path $installDir) {
    Remove-Item $installDir -Recurse -Force
}

Write-Host 'ถอนการติดตั้ง Quik-R เรียบร้อยแล้ว'
