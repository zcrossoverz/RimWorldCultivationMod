@echo off
echo Building Tu Tien mod...
cd /d "%~dp0Source\TuTien"

dotnet build --configuration Release

if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo Assembly created at: 1.6\Assemblies\TuTien.dll
) else (
    echo Build failed!
    pause
)

pause
