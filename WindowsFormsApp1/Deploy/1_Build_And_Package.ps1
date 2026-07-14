# ============================================================
#  Maansoor Hotel Hargaisa -- Deployment Package Creator
# ============================================================

$ProjectDir = Split-Path -Parent $PSScriptRoot
$OutputDir  = "$PSScriptRoot\MaansoorHotel_Setup"
$ZipFile    = "$PSScriptRoot\MaansoorHotel_Setup.zip"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MAANSOOR HOTEL -- Building Release..." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# 1. Use existing bin\Debug (VS already built it successfully)
Write-Host "`n[1/5] Using existing bin\Debug build..." -ForegroundColor Yellow
$DebugExe = "$ProjectDir\bin\Debug\WindowsFormsApp1.exe"
if (-not (Test-Path $DebugExe)) {
    Write-Host "      bin\Debug\WindowsFormsApp1.exe not found!" -ForegroundColor Red
    Write-Host "      Please build the project in Visual Studio first (Ctrl+B)." -ForegroundColor Red
    pause; exit 1
}
Write-Host "      Found: $DebugExe" -ForegroundColor Green

# 2. Create output folder
Write-Host "`n[2/5] Creating deployment folder..." -ForegroundColor Yellow
if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Path "$OutputDir\app"    | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\assets" | Out-Null

# 3. Copy application files
Write-Host "`n[3/5] Copying application files..." -ForegroundColor Yellow
$DebugDir = "$ProjectDir\bin\Debug"
if (Test-Path $DebugDir) {
    Copy-Item "$DebugDir\*" "$OutputDir\app\" -Recurse -Force
    Write-Host "      App files copied!" -ForegroundColor Green
} else {
    Write-Host "      Debug folder not found!" -ForegroundColor Red; exit 1
}

# Copy hotel logo
$LogoSrc = "$ProjectDir\..\images\mansor hotel.jpg"
if (Test-Path $LogoSrc) {
    Copy-Item $LogoSrc "$OutputDir\assets\hotel_logo.jpg" -Force
    Write-Host "      Logo copied!" -ForegroundColor Green
}

# Also copy logo to app/assets subfolder so it works directly
New-Item -ItemType Directory -Path "$OutputDir\app\assets" -Force | Out-Null
if (Test-Path $LogoSrc) {
    Copy-Item $LogoSrc "$OutputDir\app\assets\hotel_logo.jpg" -Force
}

# 4. Export database data
Write-Host "`n[4/5] Exporting database data..." -ForegroundColor Yellow
$SqlLines = New-Object System.Collections.Generic.List[string]
$SqlLines.Add("USE [hotel1];")
$SqlLines.Add("GO")
$SqlLines.Add("")

try {
    Add-Type -AssemblyName System.Data
    $conn = "Server=(LocalDB)\MSSQLLocalDB;Initial Catalog=hotel1;Integrated Security=True;Connect Timeout=10;"
    $sqlConn = New-Object System.Data.SqlClient.SqlConnection($conn)
    $sqlConn.Open()

    $tables = @("users","rooms","customers","bookings","audit_log")
    foreach ($table in $tables) {
        try {
            $SqlLines.Add("-- Table: $table")
            $cmd = $sqlConn.CreateCommand()
            $cmd.CommandText = "SELECT * FROM [$table]"
            $reader = $cmd.ExecuteReader()
            $rowCount = 0
            while ($reader.Read()) {
                $cols = @(); $vals = @()
                for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                    $cols += "[$($reader.GetName($i))]"
                    $v = $reader.GetValue($i)
                    if ($v -is [DBNull])      { $vals += "NULL" }
                    elseif ($v -is [string])   { $vals += "'" + $v.Replace("'","''") + "'" }
                    elseif ($v -is [DateTime]) { $vals += "'" + $v.ToString("yyyy-MM-dd HH:mm:ss") + "'" }
                    else                       { $vals += $v.ToString() }
                }
                $SqlLines.Add("INSERT INTO [$table] ($($cols -join ',')) VALUES ($($vals -join ','));")
                $rowCount++
            }
            $reader.Close()
            Write-Host "      Exported $rowCount rows from [$table]" -ForegroundColor Green
        } catch {
            $SqlLines.Add("-- (table $table not found or empty)")
        }
    }
    $sqlConn.Close()
} catch {
    Write-Host "      Warning: DB export skipped (fresh install OK)" -ForegroundColor Yellow
    $SqlLines.Add("-- No data exported (fresh installation)")
}

$SqlLines | Out-File "$OutputDir\data_export.sql" -Encoding UTF8
Write-Host "      data_export.sql saved!" -ForegroundColor Green

# 5. Copy installer scripts
Write-Host "`n[5/5] Adding installer scripts..." -ForegroundColor Yellow
Copy-Item "$PSScriptRoot\2_Install_On_New_PC.bat" "$OutputDir\" -Force -ErrorAction SilentlyContinue
Copy-Item "$PSScriptRoot\3_Import_Data.bat"        "$OutputDir\" -Force -ErrorAction SilentlyContinue
Copy-Item "$PSScriptRoot\README.txt"               "$OutputDir\" -Force -ErrorAction SilentlyContinue

# 6. Create ZIP
Write-Host "`n  Compressing to ZIP..." -ForegroundColor Yellow
if (Test-Path $ZipFile) { Remove-Item $ZipFile -Force }
Compress-Archive -Path "$OutputDir\*" -DestinationPath $ZipFile -Force
$SizeMB = [math]::Round((Get-Item $ZipFile).Length / 1MB, 2)

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  DONE! Package ready:" -ForegroundColor Green
Write-Host "  $ZipFile  ($SizeMB MB)" -ForegroundColor White
Write-Host ""
Write-Host "  Sidee ugu warejineysa computer cusub:" -ForegroundColor Cyan
Write-Host "  1. USB ama Google Drive ku geli ZIP-ka" -ForegroundColor White
Write-Host "  2. Furi ZIP-ka computer cusub" -ForegroundColor White
Write-Host "  3. Socodsii: 2_Install_On_New_PC.bat" -ForegroundColor White
Write-Host "  4. Socodsii: 3_Import_Data.bat (xogta)" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Green
