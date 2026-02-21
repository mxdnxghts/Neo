# NeoAndroidApp - Android Device Deployment Guide

## Quick Start

### Prerequisites

1. **.NET 10.0 SDK** or later
2. **Android SDK Platform Tools** (ADB)
3. **Android device** with USB debugging enabled

### One-Command Deploy

**Windows:**
```bash
NeoAndroidApp\deploy-to-device.bat
```

**Linux/macOS:**
```bash
chmod +x NeoAndroidApp/deploy-to-device.sh
./NeoAndroidApp/deploy-to-device.sh
```

---

## Manual Deployment Steps

### Step 1: Install Android Workload

```bash
dotnet workload install android
```

### Step 2: Enable USB Debugging on Device

1. **Enable Developer Mode:**
   - Go to Settings → About Phone
   - Tap "Build Number" 7 times
   - You'll see "You are now a developer!"

2. **Enable USB Debugging:**
   - Go to Settings → Developer Options
   - Enable "USB Debugging"
   - Connect device via USB
   - Accept the debugging prompt on your device

### Step 3: Verify Device Connection

```bash
adb devices
```

Expected output:
```
List of devices attached
ABC123XYZ    device
```

### Step 4: Build and Deploy

```bash
# Build APK
dotnet publish NeoAndroidApp/NeoAndroidApp.csproj \
    -f net10.0-android \
    -c Release \
    /p:AndroidPackageFormat=apk \
    -o ./publish

# Install APK
adb install publish/*.apk
```

### Step 5: Launch the App

The app should appear in your app drawer as "NeoAndroidApp".

---

## Deployment Script Options

| Command | Description |
|---------|-------------|
| `deploy-to-device.bat` | Windows deployment (Release) |
| `deploy-to-device.bat debug` | Windows deployment (Debug) |
| `./deploy-to-device.sh` | Linux/macOS deployment (Release) |
| `./deploy-to-device.sh debug` | Linux/macOS deployment (Debug) |

---

## Troubleshooting

### Device Not Detected

**Problem:** `No Android device detected`

**Solutions:**
1. Ensure USB cable is connected properly
2. Try a different USB port (preferably USB 2.0)
3. Install device-specific USB drivers (Windows)
4. Run `adb kill-server` then `adb start-server`
5. Check device authorization dialog

### Build Fails

**Problem:** `error XA0001: Android SDK not found`

**Solution:**
```bash
# Install Android SDK via Visual Studio or command line
# Visual Studio: Tools → Android → Android SDK Manager
# Or install via .NET:
dotnet workload install android
```

**Problem:** `The "AndroidComputeResPaths" task failed`

**Solution:** Clean and rebuild:
```bash
dotnet clean NeoAndroidApp.csproj
dotnet restore NeoAndroidApp.csproj
dotnet build -f net10.0-android -c Release NeoAndroidApp.csproj
```

### App Crashes on Launch

**View logs:**
```bash
adb logcat -s NeoAndroidApp
```

**Common issues:**
- Missing permissions in AndroidManifest.xml
- Neo.Core library incompatibility
- Tesseract tessdata files not deployed

**Clear app data:**
```bash
adb shell pm clear com.companyname.neoandroidapp
```

### APK Installation Fails

**Problem:** `INSTALL_FAILED_UPDATE_INCOMPATIBLE`

**Solution:**
```bash
# Uninstall existing version first
adb uninstall com.companyname.neoandroidapp
# Then reinstall
adb install publish/*.apk
```

**Problem:** `INSTALL_FAILED_NO_MATCHING_ABIS`

**Solution:** Build for your device's architecture:
```bash
# For ARM64 devices (most modern phones)
dotnet publish -f net10.0-android -c Release \
    /p:AndroidSupportedAbis=arm64-v8a
```

---

## Build Configurations

### Debug Build (for development)

```bash
dotnet publish NeoAndroidApp.csproj \
    -f net10.0-android \
    -c Debug \
    /p:AndroidPackageFormat=apk \
    /p:DebugType=portable \
    -o ./publish
```

**Characteristics:**
- Faster build times
- Includes debugging symbols
- Larger APK size
- Enables hot reload

### Release Build (for distribution)

```bash
dotnet publish NeoAndroidApp.csproj \
    -f net10.0-android \
    -c Release \
    /p:AndroidPackageFormat=apk \
    /p:AndroidCreatePackagePerAbi=true \
    -o ./publish
```

**Characteristics:**
- Optimized code
- Smaller APK size
- No debugging symbols
- Ready for distribution

---

## APK Output Locations

After building, APKs are located at:

```
publish/
├── com.companyname.neoandroidapp-Signed.apk    # Signed APK
└── com.companyname.neoandroidapp.apk           # Unsigned APK
```

---

## Advanced Options

### Build for Specific Architecture

```bash
# ARM64 (most modern devices)
dotnet publish -f net10.0-android -c Release \
    /p:AndroidSupportedAbis=arm64-v8a

# ARM (older devices)
dotnet publish -f net10.0-android -c Release \
    /p:AndroidSupportedAbis=armeabi-v7a

# x64 (emulators and some tablets)
dotnet publish -f net10.0-android -c Release \
    /p:AndroidSupportedAbis=x86_64
```

### Sign the APK (for distribution)

```bash
# Create keystore (first time only)
keytool -genkey -v -keystore neo-release.keystore \
    -alias neoandroidapp -keyalg RSA -keysize 2048 \
    -validity 10000

# Sign the APK
apksigner sign --ks neo-release.keystore \
    --out publish/neo-signed.apk \
    publish/com.companyname.neoandroidapp.apk
```

### Install on Multiple Devices

```bash
# Install on all connected devices
for device in $(adb devices | grep "device$" | cut -f1); do
    echo "Installing on $device..."
    adb -s "$device" install publish/*.apk
done
```

---

## Verify Installation

```bash
# Check if app is installed
adb shell pm list packages | grep neoandroidapp

# Get app info
adb shell dumpsys package com.companyname.neoandroidapp

# Check app version
adb shell dumpsys package com.companyname.neoandroidapp | grep versionName
```

---

## Uninstall

```bash
adb uninstall com.companyname.neoandroidapp
```

---

## Performance Tips

1. **Use Release configuration** for production testing
2. **Build per-ABI** to reduce APK size
3. **Enable AOT compilation** for faster startup:
   ```xml
   <PropertyGroup>
     <AndroidEnableProfiledAot>true</AndroidEnableProfiledAot>
   </PropertyGroup>
   ```
4. **Use Fast Deployment** during development:
   ```bash
   dotnet build -t:Run -f net10.0-android \
     /p:AndroidFastDeploymentType=HotReload
   ```

---

## Related Files

| File | Purpose |
|------|---------|
| `NeoAndroidApp.csproj` | Project configuration |
| `AndroidManifest.xml` | App permissions and metadata |
| `MauiProgram.cs` | App entry point and DI setup |
| `deploy-to-device.bat` | Windows deployment script |
| `deploy-to-device.sh` | Linux/macOS deployment script |

---

## Next Steps

After successful deployment:

1. **Test the app** - Enter equations and verify solutions
2. **Check logs** - Monitor for any runtime errors
3. **Optimize** - Adjust based on device performance
4. **Distribute** - Share APK or publish to app store

---

## Support

For issues related to:
- **.NET MAUI**: https://docs.microsoft.com/dotnet/maui/
- **Android SDK**: https://developer.android.com/studio/intro
- **ADB Commands**: https://developer.android.com/studio/command-line/adb
