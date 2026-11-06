# Publishes PaperFlow (self-contained) and builds the Inno Setup installer.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$publish = Join-Path $PSScriptRoot 'publish'
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }

dotnet publish "$root\PaperFlow\PaperFlow.csproj" -c Release -r win-x64 -p:Platform=x64 -p:PublishProfile= -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

$iscc = @(
  "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 not found' }

& $iscc "$PSScriptRoot\PaperFlow.iss"
if ($LASTEXITCODE -ne 0) { throw 'ISCC failed' }
