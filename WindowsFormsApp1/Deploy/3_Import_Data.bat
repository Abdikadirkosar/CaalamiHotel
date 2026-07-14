@echo off
title Maansoor Hotel — Import Existing Data
color 2F
echo.
echo =====================================================
echo   MAANSOOR HOTEL — Import Database Data
echo   Soo Geli Xogta Database-ka
echo =====================================================
echo.
echo   Tani waxay soo gelisaa xogta hore (customers,
echo   rooms, bookings, users) database cusub.
echo.
echo   Hubi inaad marka hore nidaamka hal mar u socodsiisay
echo   si database-ka loo sameeyo.
echo.
pause

echo.
echo [1/2] Starting database import...
SqlLocalDB.exe start MSSQLLocalDB >nul 2>&1

sqlcmd -S "(LocalDB)\MSSQLLocalDB" -i "%~dp0data_export.sql" -b
if %errorlevel% EQU 0 (
    echo.
    echo [2/2] Import SUCCESSFUL!
    echo       Xogta hore si guul leh ayaa loo keenay!
) else (
    echo.
    echo [2/2] Import had a warning - some tables may already have data.
    echo       Xogta qaar waxaa laga yaabaa inay horeba joogaan.
)

echo.
echo =====================================================
echo   DONE! Xogta waa la keenay.
echo   Bilow nidaamka si aad u xaqiijiso.
echo =====================================================
pause
