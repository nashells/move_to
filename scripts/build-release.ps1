param(
    [string]$OutputDir = $(Join-Path (Split-Path $PSScriptRoot -Parent) "Release"),
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent

Write-Host "Building release package..." -ForegroundColor Cyan

# Clean output directory if requested
if ($Clean -and (Test-Path $OutputDir)) {
    Remove-Item -Path $OutputDir -Recurse -Force
}

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Build Release configuration
Write-Host "Building MoveTo.Shell (Release)..." -ForegroundColor Yellow
$buildResult = & dotnet build "$RepoRoot\src\MoveTo.Shell\MoveTo.Shell.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Copy binaries
$binSource = Join-Path $RepoRoot "src\MoveTo.Shell\bin\Release\net48"
$binDest = Join-Path $OutputDir "bin"
New-Item -ItemType Directory -Force -Path $binDest | Out-Null

Write-Host "Copying binaries..." -ForegroundColor Yellow
$dlls = Get-ChildItem -Path $binSource -Filter *.dll
foreach ($dll in $dlls) {
    Copy-Item -Path $dll.FullName -Destination $binDest -Force
}
$configs = Get-ChildItem -Path $binSource -Filter *.config -ErrorAction SilentlyContinue
foreach ($cfg in $configs) {
    Copy-Item -Path $cfg.FullName -Destination $binDest -Force
}

# Copy scripts from templates
$templatesDir = Join-Path $PSScriptRoot "release-templates"
Copy-Item -Path (Join-Path $templatesDir "install.ps1") -Destination $OutputDir -Force
Copy-Item -Path (Join-Path $templatesDir "uninstall.ps1") -Destination $OutputDir -Force
Copy-Item -Path (Join-Path $templatesDir "README.md") -Destination $OutputDir -Force

Write-Host ""
Write-Host "Release package created successfully!" -ForegroundColor Green
Write-Host "Output: $OutputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Contents:" -ForegroundColor Yellow
Get-ChildItem -Path $OutputDir -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring($OutputDir.Length + 1)
    if ($_.PSIsContainer) {
        Write-Host "  [DIR] $relativePath"
    } else {
        Write-Host "  $relativePath ($([math]::Round($_.Length / 1KB, 1)) KB)"
    }
}
