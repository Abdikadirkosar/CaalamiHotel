@echo off
echo ===================================================
echo   🏨 HOTEL SYSTEM - AUTOMATIC RESX FILE UNBLOCKER
echo ===================================================
echo.
echo Deleting 'Mark of the Web' security flags...
echo.

powershell -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0WindowsFormsApp1' -Recurse | Unblock-File"

echo.
echo ===================================================
echo   ✅ DONE! Files have been unblocked successfully.
echo   Please go back to Visual Studio and click REBUILD.
echo ===================================================
echo.
pause
