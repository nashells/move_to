param(
    [string]$InstallDir = "C:\Program Files\nashells\MoveTo",
    [string]$RegasmPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
)

$ErrorActionPreference = "Stop"

function Require-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this script as Administrator."
    }
}

function Copy-Binaries {
    param([string]$from, [string]$to)
    if (-not (Test-Path $from)) { throw "Source not found: $from" }
    New-Item -ItemType Directory -Force -Path $to | Out-Null
    $dlls = Get-ChildItem -Path $from -Filter *.dll
    if (-not $dlls) { throw "No DLLs found in $from" }
    foreach ($dll in $dlls) { Copy-Item -Path $dll.FullName -Destination $to -Force }
    $configs = Get-ChildItem -Path $from -Filter *.config -ErrorAction SilentlyContinue
    foreach ($cfg in $configs) { Copy-Item -Path $cfg.FullName -Destination $to -Force }
}

function Register-Server {
    param([string]$dllPath, [string]$regasm)
    if (-not (Test-Path $regasm)) { throw "regasm not found: $regasm" }
    if (-not (Test-Path $dllPath)) { throw "DLL not found: $dllPath" }
    & $regasm /codebase $dllPath
    if ($LASTEXITCODE -ne 0) { throw "regasm failed with exit code $LASTEXITCODE" }
}

function Add-ApprovedEntry {
    $key = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved'
    $name = '{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}'
    $value = 'MoveTo context menu'
    New-Item -Path $key -Force | Out-Null
    New-ItemProperty -Path $key -Name $name -Value $value -PropertyType String -Force | Out-Null
}

function Register-ContextMenuHandler {
    $guid = '{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}'
    & reg add "HKCR\*\shellex\ContextMenuHandlers\MoveTo" /ve /d $guid /f | Out-Null
    & reg add "HKCR\Directory\shellex\ContextMenuHandlers\MoveTo" /ve /d $guid /f | Out-Null
}

function Ensure-Config {
    param([string]$configPath)
    $dir = Split-Path $configPath -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    if (-not (Test-Path $configPath)) {
        $template = '{
  "destinations": [
    { "displayName": "Temp", "path": "C:\\Temp" }
  ]
}'
        Set-Content -Path $configPath -Value $template -Encoding UTF8
    }
}

Require-Admin

# Stop Explorer to release DLL locks
Write-Host "Stopping Explorer..." -ForegroundColor Yellow
Get-Process explorer -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# Copy binaries
$sourceDir = Join-Path $PSScriptRoot "bin"
Copy-Binaries -from $sourceDir -to $InstallDir

# Register COM server
$dll = Join-Path $InstallDir "MoveTo.Shell.dll"
Write-Host "Registering COM server..." -ForegroundColor Yellow
Register-Server -dllPath $dll -regasm $RegasmPath
Add-ApprovedEntry
Register-ContextMenuHandler

# Create default config
$configPath = Join-Path $env:LOCALAPPDATA "MoveTo\config.json"
Ensure-Config -configPath $configPath

# Restart Explorer
Write-Host "Restarting Explorer..." -ForegroundColor Yellow
Start-Process explorer.exe

Write-Host "Install completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Configuration file: $configPath" -ForegroundColor Cyan
Write-Host "Edit this file to add your destination folders." -ForegroundColor Cyan
