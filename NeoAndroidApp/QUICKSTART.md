# NeoAndroidApp - Quick Start Guide for Physical Device

## ✅ Setup Complete

The project has been configured and built successfully for Android deployment.

---

## 📦 Build Output

**APK Location:** `G:\Neo\NeoAndroidApp\bin\Debug\net10.0-android\`
- `com.companyname.neoandroidapp-Signed.apk` (~61 MB)
- `com.companyname.neoandroidapp.apk` (~61 MB)

---

## 🚀 Deploy to Device (Quick Method)

### Option 1: Using Deployment Script

```bash
cd G:\Neo\NeoAndroidApp
deploy-to-device.bat
```

### Option 2: Manual Deployment

1. **Connect your Android device** via USB
2. **Enable USB Debugging** on device
3. **Run commands:**

```bash
# Navigate to project
cd G:\Neo\NeoAndroidApp

# Build APK
dotnet publish -c Release /p:AndroidPackageFormat=apk

# Install APK
adb install bin\Release\net10.0-android\android-arm64\publish\*.apk
```

---

## 📱 Connect Physical Device

### Step 1: Enable Developer Mode

1. Go to **Settings** → **About Phone**
2. Tap **Build Number** 7 times
3. You'll see "You are now a developer!"

### Step 2: Enable USB Debugging

1. Go to **Settings** → **Developer Options**
2. Enable **USB Debugging**
3. Connect device via USB
4. Accept the USB debugging prompt on your device

### Step 3: Verify Connection

```bash
adb devices
```

Expected output:
```
List of devices attached
ABC123XYZ    device
```

---

## 🔧 Environment Configuration

### Installed Components

✅ .NET Android SDK 36.1.30
✅ .NET MAUI SDK 10.0.20
✅ Android Build Tools 35.0.0, 36.0.0
✅ Android Platforms 33, 34, 35, 36
✅ ADB (Android Debug Bridge)

### Environment Variables

```
ANDROID_HOME=G:\Apps\android-sdk
NUGET_PACKAGES=G:\NuGetPackages
```

---

## 📋 Project Configuration

### Target Framework
```xml
<TargetFramework>net10.0-android</TargetFramework>
```

### Supported ABIs
- `armeabi-v7a` (32-bit ARM)
- `arm64-v8a` (64-bit ARM) ← Most modern devices
- `x86` (32-bit x86 emulator)
- `x86_64` (64-bit x86 emulator)

### Minimum Android Version
- API Level 21 (Android 5.0 Lollipop)

---

## 🛠️ Build Commands

### Debug Build (for development)
```bash
dotnet build -c Debug -f net10.0-android
```

### Release Build (for distribution)
```bash
dotnet build -c Release -f net10.0-android
```

### Publish APK
```bash
dotnet publish -c Release /p:AndroidPackageFormat=apk
```

---

## 📲 Install Commands

### Install on connected device
```bash
adb install bin\Debug\net10.0-android\com.companyname.neoandroidapp-Signed.apk
```

### Install and run
```bash
adb install -r bin\Debug\net10.0-android\com.companyname.neoandroidapp-Signed.apk
adb shell am start -n com.companyname.neoandroidapp/crc641234567890.MainActivity
```

### Uninstall
```bash
adb uninstall com.companyname.neoandroidapp
```

---

## 🐛 Troubleshooting

### Device not detected
```bash
# Restart ADB server
adb kill-server
adb start-server

# Check again
adb devices
```

### Installation failed (INSUFFICIENT_STORAGE)
```bash
# Clear app cache
adb shell pm clear com.companyname.neoandroidapp
```

### App crashes on launch
```bash
# View logs
adb logcat -s NeoAndroidApp

# Clear app data
adb shell pm clear com.companyname.neoandroidapp
```

### Build errors
```bash
# Clean and rebuild
dotnet clean NeoAndroidApp.csproj
dotnet restore NeoAndroidApp.csproj
dotnet build -c Debug -f net10.0-android
```

---

## 📊 Performance Tips

### For faster builds during development:
```bash
# Use Debug configuration
dotnet build -c Debug

# Use shared runtime
dotnet build /p:AndroidUseSharedRuntime=true
```

### For smaller APK size:
```bash
# Build for specific ABI (e.g., arm64 only)
dotnet publish -c Release /p:AndroidSupportedAbis=arm64-v8a
```

---

## 📝 Files Modified

| File | Changes |
|------|---------|
| `NeoAndroidApp.csproj` | Changed to Android-only target |
| `MainActivity.cs` | Removed unused IronOcr import |
| `deploy-to-device.bat` | Updated with correct Android SDK path |
| `DEPLOYMENT.md` | Full deployment documentation |
| `NuGet.Config` | Redirected NuGet cache to G: drive |

---

## 🎯 Next Steps

1. **Connect your device** following the steps above
2. **Run the deployment script**: `deploy-to-device.bat`
3. **Launch the app** from your device's app drawer
4. **Test the equation solver** functionality

---

## 📖 Additional Resources

- [DEPLOYMENT.md](./DEPLOYMENT.md) - Comprehensive deployment guide
- [ARCHITECTURE.md](./ARCHITECTURE.md) - App architecture documentation
- [IMPLEMENTATION.md](./IMPLEMENTATION.md) - Implementation details

---

**Last Updated:** February 21, 2026
**Build Status:** ✅ Successful (195 warnings, 0 errors)
