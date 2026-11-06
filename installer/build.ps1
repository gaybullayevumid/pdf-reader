# Publishes PDF Reader (self-contained, win-x64) and builds the Inno Setup installer.
#   .\installer\build.ps1              -> installer\output\PDFReader-Setup-<version>.exe
#   .\installer\build.ps1 -Portable    -> also installer\output\PDFReader-<version>-win-x64-portable.zip
param([switch]$Portable)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$publish = Join-Path $PSScriptRoot 'publish'
$output = Join-Path $PSScriptRoot 'output'
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }

dotnet publish "$root\PdfReader.csproj" -c Release -r win-x64 -p:Platform=x64 -p:PublishProfile= -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

$iscc = @(
  "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 not found (https://jrsoftware.org/isdl.php)' }

$version = ([xml](Get-Content "$root\PdfReader.csproj")).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw 'No <Version> in PdfReader.csproj' }
Write-Host "Version: $version"

& $iscc "/DAppVersion=$version" "$PSScriptRoot\PdfReader.iss"
if ($LASTEXITCODE -ne 0) { throw 'ISCC failed' }

if ($Portable) {
  $zip = Join-Path $output "PDFReader-$version-win-x64-portable.zip"
  if (Test-Path $zip) { Remove-Item $zip }
  Compress-Archive -Path "$publish\*" -DestinationPath $zip
  Write-Host "Portable build: $zip"
}
