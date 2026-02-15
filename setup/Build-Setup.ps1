$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$iscc = 'C:\Users\BDC\AppData\Local\Programs\Inno Setup 6\ISCC.exe'

if (-not (Test-Path $dotnet)) {
    throw 'ไม่พบ dotnet.exe ที่ C:\Program Files\dotnet\dotnet.exe'
}

if (-not (Test-Path $iscc)) {
    throw 'ไม่พบ ISCC.exe กรุณาติดตั้ง Inno Setup 6 ก่อน'
}

Start-Process -FilePath $dotnet -ArgumentList "publish `"$projectRoot\QuickReply.csproj`" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true" -Wait -NoNewWindow

$issPath = Join-Path $PSScriptRoot 'QuikR.iss'
Start-Process -FilePath $iscc -ArgumentList "`"$issPath`"" -Wait -NoNewWindow

$output = Join-Path $projectRoot 'dist\Quik-R-Setup.exe'
if (Test-Path $output) {
    Write-Host "Build installer success: $output"
} else {
    throw 'สร้าง Setup ไม่สำเร็จ (ไม่พบไฟล์ output)'
}
