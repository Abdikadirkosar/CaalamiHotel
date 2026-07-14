# ZipSource.ps1
$SourceDir = "c:\Users\r9cb09\OneDrive\Desktop\Hotel-Management-System-main"
$ZipPath = "c:\Users\r9cb09\OneDrive\Desktop\MaansoorHotel_SourceCode.zip"

Write-Host "Zipping source code from $SourceDir to $ZipPath..." -ForegroundColor Cyan

# Create a temporary directory to copy clean files
$TempDir = Join-Path $env:TEMP "MaansoorSourceTemp"
if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
New-Item -ItemType Directory -Path $TempDir | Out-Null

# Copy everything except build files and temporary configurations
Get-ChildItem -Path $SourceDir -Recurse | Where-Object {
    $_.FullName -notmatch '\\bin\\' -and 
    $_.FullName -notmatch '\\obj\\' -and 
    $_.FullName -notmatch '\\\.vs\\' -and
    $_.FullName -notmatch '\\MaansoorHotel_Setup\\'
} | Copy-Item -Destination {
    $relPath = $_.FullName.Substring($SourceDir.Length + 1)
    $dest = Join-Path $TempDir $relPath
    $parent = Split-Path $dest
    if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent | Out-Null }
    $dest
} -Force -ErrorAction SilentlyContinue

# Create ZIP
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Compress-Archive -Path "$TempDir\*" -DestinationPath $ZipPath -Force

# Clean up
Remove-Item $TempDir -Recurse -Force

Write-Host "SUCCESS! Clean source code zipped at: $ZipPath" -ForegroundColor Green
