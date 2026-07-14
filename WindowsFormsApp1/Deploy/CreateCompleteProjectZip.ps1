# CreateCompleteProjectZip.ps1
$SourceDir = "c:\Users\r9cb09\OneDrive\Desktop\Hotel-Management-System-main"
$ZipPath = "c:\Users\r9cb09\OneDrive\Desktop\MaansoorHotel_VS_Project.zip"
$UsbZipPath = "D:\MaansoorHotel_VS_Project.zip"

Write-Host "Preparing complete Visual Studio Project ZIP..." -ForegroundColor Cyan

# Create a clean copy in Temp, keeping .sln and source files, but removing locked/temporary compiler files
$TempDir = Join-Path $env:TEMP "VSProjectTemp"
if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
New-Item -ItemType Directory -Path $TempDir | Out-Null

# Copy files safely
Get-ChildItem -Path $SourceDir -Recurse | Where-Object {
    $_.FullName -notmatch '\\\.vs\\' -and
    $_.FullName -notmatch '\\bin\\Release\\' -and
    $_.FullName -notmatch '\\obj\\Release\\' -and
    $_.FullName -notmatch '\\Deploy\\'
} | Copy-Item -Destination {
    $relPath = $_.FullName.Substring($SourceDir.Length + 1)
    $dest = Join-Path $TempDir $relPath
    $parent = Split-Path $dest
    if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent | Out-Null }
    $dest
} -Force -ErrorAction SilentlyContinue

# Compress
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Compress-Archive -Path "$TempDir\*" -DestinationPath $ZipPath -Force

# Copy to USB if available
if (Test-Path "D:\") {
    if (Test-Path $UsbZipPath) { Remove-Item $UsbZipPath -Force }
    Copy-Item $ZipPath $UsbZipPath -Force
    Write-Host "Copied to USB (D:) successfully!" -ForegroundColor Green
}

# Clean up
Remove-Item $TempDir -Recurse -Force

Write-Host "SUCCESS! Complete Visual Studio Project saved at:" -ForegroundColor Green
Write-Host "1. $ZipPath" -ForegroundColor White
Write-Host "2. $UsbZipPath" -ForegroundColor White
