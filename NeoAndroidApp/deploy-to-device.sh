#!/usr/bin/env bash
# ============================================================
# NeoAndroidApp - Deploy to Physical Android Device
# ============================================================
# This script builds and deploys the app to a connected device
# ============================================================

set -e

echo ""
echo "============================================"
echo " NeoAndroidApp - Android Device Deployment"
echo "============================================"
echo ""

# Check if ADB is available
if ! command -v adb &> /dev/null; then
    echo "[ERROR] ADB not found in PATH."
    echo "Please install Android SDK Platform Tools."
    echo "Download: https://developer.android.com/studio/releases/platform-tools"
    exit 1
fi

# Check if device is connected
echo "[INFO] Checking for connected devices..."
if ! adb devices | grep -q "device$"; then
    echo "[ERROR] No Android device detected."
    echo ""
    echo "Please:"
    echo "  1. Enable Developer Mode on your device"
    echo "  2. Enable USB Debugging in Developer Options"
    echo "  3. Connect device via USB"
    echo "  4. Accept the USB debugging prompt on your device"
    exit 1
fi

DEVICE=$(adb devices | grep "device$" | head -1 | cut -f1)
echo "[INFO] Device found: $DEVICE"
echo ""

# Check if .NET Android workload is installed
echo "[INFO] Checking .NET Android workload..."
if ! dotnet workload list 2>/dev/null | grep -qi "android"; then
    echo "[WARN] Android workload not detected. Attempting to install..."
    dotnet workload install android
fi

# Get build configuration
CONFIG="Release"
if [ "$1" == "debug" ]; then
    CONFIG="Debug"
fi

echo "[INFO] Building in $CONFIG configuration..."
echo ""

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Build the APK
dotnet publish NeoAndroidApp.csproj \
    -f net10.0-android \
    -c "$CONFIG" \
    /p:AndroidPackageFormat=apk \
    /p:ApplicationVersion=1 \
    /p:ApplicationDisplayVersion=1.0 \
    -o ./publish

if [ $? -ne 0 ]; then
    echo "[ERROR] Build failed. Check the errors above."
    exit 1
fi

echo ""
echo "[INFO] Build successful!"
echo ""

# Find the APK file
APK_PATH=$(find ./publish -name "*.apk" -type f | head -1)

if [ -z "$APK_PATH" ]; then
    echo "[ERROR] APK file not found after build."
    exit 1
fi

echo "[INFO] APK found: $APK_PATH"
echo ""

# Uninstall existing version (if any)
echo "[INFO] Removing existing installation (if any)..."
adb -s "$DEVICE" uninstall com.companyname.neoandroidapp || true
echo ""

# Install the new APK
echo "[INFO] Installing APK to device..."
adb -s "$DEVICE" install "$APK_PATH"

if [ $? -ne 0 ]; then
    echo "[ERROR] Installation failed."
    exit 1
fi

echo ""
echo "============================================"
echo " Deployment Successful!"
echo "============================================"
echo ""
echo "App installed: com.companyname.neoandroidapp"
echo ""
echo "To view logs, run:"
echo "  adb logcat -s NeoAndroidApp"
echo ""
echo "To start the app, run:"
echo "  adb shell am start -n com.companyname.neoandroidapp/crc641234567890.MainActivity"
echo ""
