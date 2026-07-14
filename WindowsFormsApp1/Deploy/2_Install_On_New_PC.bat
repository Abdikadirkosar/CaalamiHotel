@echo off
title Maansoor Hotel Hargaisa — Setup on New PC
color 1F
echo.
echo =====================================================
echo   MAANSOOR HOTEL HARGAISA - System Installation
echo   Xulumada Nidaamka - Computer Cusub
echo =====================================================
echo.

:: ── Step 1: Check .NET Framework 4.8 ─────────────────────
echo [1/4] Checking .NET Framework 4.8...
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release >nul 2>&1
if %errorlevel% NEQ 0 (
    echo       NOT FOUND. Please install .NET Framework 4.8 first.
    echo       Download: https://dotnet.microsoft.com/download/dotnet-framework/net48
    pause
    start https://dotnet.microsoft.com/download/dotnet-framework/net48
    exit /b 1
)
for /f "tokens=3" %%v in ('reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release 2^>nul') do set RELEASE=%%v
if %RELEASE% GEQ 528040 (
    echo       .NET 4.8 OK!
) else (
    echo       .NET version too old. Please update.
    pause
    exit /b 1
)

:: ── Step 2: Check SQL Server LocalDB ─────────────────────
echo.
echo [2/4] Checking SQL Server LocalDB...
SqlLocalDB.exe info MSSQLLocalDB >nul 2>&1
if %errorlevel% NEQ 0 (
    echo       LocalDB not found. Installing...
    echo       Download: https://aka.ms/sqllocaldb
    pause
    start https://aka.ms/sqllocaldb
    exit /b 1
) else (
    echo       SQL Server LocalDB OK!
)

:: ── Step 3: Copy application files ───────────────────────
echo.
echo [3/4] Installing application files...
set INSTALL_DIR=%LOCALAPPDATA%\MaansoorHotel
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%\assets" mkdir "%INSTALL_DIR%\assets"

xcopy /E /Y /Q "%~dp0app\*"    "%INSTALL_DIR%\" >nul
xcopy /E /Y /Q "%~dp0assets\*" "%INSTALL_DIR%\assets\" >nul
echo       Files copied to: %INSTALL_DIR%

:: ── Step 4: Create Desktop Shortcut ──────────────────────
echo.
echo [4/4] Creating Desktop shortcut...
set SHORTCUT=%USERPROFILE%\Desktop\Maansoor Hotel.lnk
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $sc = $ws.CreateShortcut('%SHORTCUT%'); $sc.TargetPath = '%INSTALL_DIR%\WindowsFormsApp1.exe'; $sc.Description = 'Maansoor Hotel Hargaisa Management System'; $sc.Save()"
echo       Shortcut created on Desktop!

:: ── Done! ────────────────────────────────────────────────
echo.
echo =====================================================
echo   INSTALLATION COMPLETE!
echo   Nidaamka waa la rakibay!
echo.
echo   Fur shortcut-ka Desktop-ka si aad u bilowdo.
echo   Double-click: "Maansoor Hotel.lnk"
echo.
echo   Xogta hore (data) haddaad raabtid, fur:
echo   3_Import_Data.bat
echo =====================================================
echo.
pause
start "" "%INSTALL_DIR%\WindowsFormsApp1.exe"
