param(
    [string]$InstallDir = "C:\Program Files\nashells\MoveTo",
    [string]$RegasmPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe",
    [switch]$RemoveFiles,
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

function Unregister-Server {
    param([string]$dllPath, [string]$regasm)
    if (-not (Test-Path $regasm)) { throw "regasm not found: $regasm" }
    if (-not (Test-Path $dllPath)) { Write-Warning "DLL not found (skip unregister): $dllPath"; return }
    & $regasm /unregister $dllPath
    if ($LASTEXITCODE -ne 0) { throw "regasm unregister failed with exit code $LASTEXITCODE" }
}

function Remove-ApprovedEntry {
    $key = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved'
    $name = '{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}'
    if (Test-Path $key) {
        Remove-ItemProperty -Path $key -Name $name -ErrorAction SilentlyContinue
    }
}

function Remove-ContextMenuHandler {
    # AllFiles (*) の登録削除
    & reg delete "HKCR\*\shellex\ContextMenuHandlers\MoveTo" /f 2>$null
    
    # Directory の登録削除
    & reg delete "HKCR\Directory\shellex\ContextMenuHandlers\MoveTo" /f 2>$null
}

Require-Admin
$dll = Join-Path $InstallDir "MoveTo.Shell.dll"
Unregister-Server -dllPath $dll -regasm $RegasmPath
Remove-ApprovedEntry
Remove-ContextMenuHandler

if ($RemoveFiles -and (Test-Path $InstallDir)) {
    Remove-Item -Path $InstallDir -Recurse -Force
}

if ($RestartExplorer) {
    Get-Process explorer -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Process explorer.exe
}

Write-Host "Uninstall completed."