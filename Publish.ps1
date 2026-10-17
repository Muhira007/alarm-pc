$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet publish AlarmGiliran.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o dist
    if ($LASTEXITCODE -ne 0) { throw 'Build aplikasi gagal.' }
    Copy-Item -LiteralPath README.md -Destination dist\Panduan.md
    Write-Host 'Selesai: dist\AlarmGiliran.exe'
} finally { Pop-Location }
