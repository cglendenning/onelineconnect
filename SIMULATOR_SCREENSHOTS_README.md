# Simulator Screenshots Guide for One Line Connect

## Overview

This guide explains how to take clean App Store screenshots using the iOS Simulator without IAP or ad errors.

## What Was Added

### 1. Simulator Detection System
- **`SimulatorDetector.cs`**: Utility class that detects when the app is running in the iOS Simulator
- **`SimulatorDetector.mm`**: Native iOS plugin for accurate simulator detection
- **`SimulatorTest.cs`**: Test script to verify simulator detection is working

### 2. Modified Components
- **`Purchaser.cs`**: IAP initialization and purchases are skipped in simulator
- **`AdmobController.cs`**: All ad requests and displays are blocked in simulator
- **`CUtils.cs`**: Ad-related methods check for simulator before executing
- **`UIControllerForGame.cs`**: Banner ad calls are stopped in simulator

## How It Works

### Simulator Detection
The system uses a native iOS plugin to accurately detect when running in the iOS Simulator:

```csharp
bool isSimulator = SimulatorDetector.IsRunningInSimulator();
```

### IAP Handling in Simulator
- IAP initialization is skipped completely
- Purchase attempts are logged but not executed
- No error messages are displayed to the user
- App continues to function normally

### Ad Handling in Simulator
- All ad requests are blocked
- Banner ads are not displayed
- Interstitial ads are not shown
- Rewarded video ads are disabled
- Clean UI without ad overlays

## Taking Screenshots

### Step 1: Build for iOS
1. In Unity, go to `File > Build Settings`
2. Select `iOS` platform
3. Click `Build` and choose a folder (e.g., `iOSBuild`)

### Step 2: Launch Simulator
```bash
# Navigate to your build folder
cd /Users/craig/development/games/one_line_connect/iOSBuild

# Boot iPhone 16 Pro Max simulator (6.9" display)
xcrun simctl boot "iPhone 16 Pro Max"

# Install your app
xcrun simctl install "iPhone 16 Pro Max" Unity-iPhone.app

# Launch your app
xcrun simctl launch "iPhone 16 Pro Max" com.yourcompany.onelineconnect
```

### Step 3: Take Screenshots
The app will now run without IAP or ad errors. You can:

1. **Use Simulator's built-in screenshot**: `Cmd + S`
2. **Use macOS screenshot tool**: `Cmd + Shift + 4`
3. **Use terminal command**:
   ```bash
   xcrun simctl io "iPhone 16 Pro Max" screenshot ~/Desktop/screenshot1.png
   ```

### Step 4: Required Screenshots
Take screenshots of these key screens:
1. **Main Menu** - Play screen with game title
2. **World Selection** - Available worlds/packages
3. **Level Selection** - Grid of levels
4. **Active Gameplay** - Mid-game with puzzle visible
5. **Victory Screen** - Level completion celebration
6. **Pause Menu** - Game paused with options

## Testing the System

### Keyboard Shortcuts (in simulator)
- **`T`**: Test IAP functionality
- **`A`**: Test ad functionality  
- **`S`**: Show simulator status

### Console Logs
Look for these log messages to verify simulator detection:
```
=== SIMULATOR DETECTION ===
Running in simulator: YES
Platform: iPhonePlayer
Is Editor: NO
iOS Platform: YES
```

### Expected Behavior in Simulator
- **IAP buttons**: Will log purchase attempts but not execute
- **Ads**: Will be blocked with log messages
- **UI**: Clean interface without ad overlays
- **Performance**: Normal game performance

## Troubleshooting

### If IAP Still Shows Errors
1. Check that `SimulatorDetector.IsRunningInSimulator()` returns `true`
2. Verify the native plugin `SimulatorDetector.mm` is included in the build
3. Check console logs for simulator detection messages

### If Ads Still Appear
1. Ensure `CUtils.ShowBannerAd()` and `CUtils.ShowInterstitialAd()` are being called
2. Check that `AdmobController` is properly detecting simulator
3. Verify no ad requests are being made in console logs

### Build Issues
1. Make sure the iOS plugin is in `Assets/Plugins/iOS/`
2. Check that the plugin has proper meta files
3. Verify iOS build settings include the plugin

## File Structure

```
Assets/
├── OneLine/
│   └── MyCombo/
│       ├── SimulatorDetector.cs          # Main detection utility
│       ├── SimulatorTest.cs              # Test script
│       ├── Purchaser.cs                  # Modified IAP handling
│       ├── AdmobController.cs            # Modified ad handling
│       └── CUtils.cs                     # Modified ad utilities
└── Plugins/
    └── iOS/
        └── SimulatorDetector.mm          # Native iOS plugin
```

## Verification Checklist

Before taking screenshots, verify:
- [ ] Console shows "Running in simulator: YES"
- [ ] No IAP initialization errors
- [ ] No ad loading errors
- [ ] UI is clean without ad overlays
- [ ] Game functionality works normally
- [ ] Screenshots are 1290 x 2796 pixels (iPhone 16 Pro Max)

## Notes

- The simulator detection only affects IAP and ads
- All other game functionality remains unchanged
- The system automatically detects simulator vs. real device
- No manual configuration required
- Works with all iOS simulator devices

This system ensures you can take professional App Store screenshots without any IAP or ad-related errors or overlays. 