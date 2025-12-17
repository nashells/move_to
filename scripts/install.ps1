param(
    [string]$InstallDir = "C:\Program Files\nashells\MoveTo",
    [string]$RegasmPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe",
    [string]$SourceDir = $(Join-Path (Split-Path $PSScriptRoot -Parent) "src\MoveTo.Shell\bin\Release\net8.0-windows"),
    [switch]$SkipCopy,
    [switch]$RestartExplorer
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

function Ensure-Config {
    param([string]$configPath)
    $dir = Split-Path $configPath -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    if (-not (Test-Path $configPath)) {
        $template = @{destinations=@(@{displayName="Temp";path="C:\\Temp"})} | ConvertTo-Json -Depth 4
        Set-Content -Path $configPath -Value $template -Encoding UTF8
    }
}

Require-Admin
if (-not $SkipCopy) { Copy-Binaries -from $SourceDir -to $InstallDir }

$dll = Join-Path $InstallDir "MoveTo.Shell.dll"
Register-Server -dllPath $dll -regasm $RegasmPath

$configPath = Join-Path $env:LOCALAPPDATA "MoveTo\config.json"
Ensure-Config -configPath $configPath

if ($RestartExplorer) {
    Get-Process explorer -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Process explorer.exe
}

Write-Host "Install completed."