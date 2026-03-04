@echo off
REM ============================================================
REM NeoAndroidApp - Deploy to Physical Android Device
REM ============================================================
REM This script builds and deploys the app to a connected device
REM ============================================================

setlocal enabledelayedexpansion

REM Set Android SDK path if not already set
if "%ANDROID_HOME%"=="" set ANDROID_HOME=C:\Program Files (x86)\Android\android-sdk
if "%ANDROID_SDK_ROOT%"=="" set ANDROID_SDK_ROOT=%ANDROID_HOME%

REM Add ADB to PATH
set PATH=%ANDROID_HOME%\platform-tools;%PATH%

echo.
echo ============================================
echo  NeoAndroidApp - Android Device Deployment
echo ============================================
echo.
echo [INFO] ANDROID_HOME=%ANDROID_HOME%
echo.

REM Check if ADB is available
where adb >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] ADB not found in PATH.
    echo Using Android SDK at: %ANDROID_HOME%\platform-tools
    exit /b 1
)

REM Check if device is connected
echo [INFO] Checking for connected devices...
adb devices >nul 2>nul
for /f "skip=1 tokens=1" %%i in ('adb devices') do (
    if not "%%i"=="" set DEVICE_FOUND=%%i
)

if "%DEVICE_FOUND%"=="" (
    echo [ERROR] No Android device detected.
    echo.
    echo Please:
    echo   1. Enable Developer Mode on your device
    echo   2. Enable USB Debugging in Developer Options
    echo   3. Connect device via USB
    echo   4. Accept the USB debugging prompt on your device
    exit /b 1
)

echo [INFO] Device found: %DEVICE_FOUND%
echo.

REM Check if .NET Android workload is installed
echo [INFO] Checking .NET Android workload...
dotnet workload list | findstr /i "android" >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [WARN] Android workload not detected. Attempting to install...
    dotnet workload install android
    if %ERRORLEVEL% neq 0 (
        echo [ERROR] Failed to install Android workload.
        echo Run: dotnet workload install android
        exit /b 1
    )
)

REM Get build configuration
set CONFIG=Release
if "%1"=="debug" set CONFIG=Debug

echo [INFO] Building in %CONFIG% configuration...
echo.

REM Build the APK
dotnet publish NeoAndroidApp.csproj ^
    -c %CONFIG% ^
    /p:AndroidPackageFormat=apk ^
    /p:ApplicationVersion=1 ^
    /p:ApplicationDisplayVersion=1.0 ^
    -o ./publish

if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed. Check the errors above.
    exit /b 1
)

echo.
echo [INFO] Build successful!
echo.

REM Find the APK file
for /f "delims=" %%f in ('dir /b /s publish\*.apk 2^>nul') do (
    set APK_PATH=%%f
    goto :found
)

echo [ERROR] APK file not found after build.
exit /b 1

:found
echo [INFO] APK found: %APK_PATH%
echo.

REM Uninstall existing version (if any)
echo [INFO] Removing existing installation (if any)...
adb -s %DEVICE_FOUND% uninstall com.companyname.neoandroidapp
echo.

REM Install the new APK
echo [INFO] Installing APK to device...
adb -s %DEVICE_FOUND% install "%APK_PATH%"

if %ERRORLEVEL% neq 0 (
    echo [ERROR] Installation failed.
    exit /b 1
)

echo.
echo ============================================
echo  Deployment Successful!
echo ============================================
echo.
echo App installed: com.companyname.neoandroidapp
echo.
echo To view logs, run:
echo   adb logcat -s NeoAndroidApp
echo.
echo To start the app, run:
echo   adb shell am start -n com.companyname.neoandroidapp/crc641234567890.MainActivity
echo.

exit /b 0
