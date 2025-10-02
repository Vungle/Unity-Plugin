# Liftoff Windows Unity Wrapper for the v7 SDK

This package contains:
- **Assets/Scripts**: Platform-safe `LiftoffWindows.cs` and `LiftoffSample.cs`.
- **Assets/Plugins/x86_64**: Place `LiftoffUnityBridge.dll`, `WebView2Loader.dll`, and vendor SDK DLLs here.
- **plugin/**: C++ bridge source and a Visual Studio solution for building `LiftoffUnityBridge.dll` (x64).

## Build the Plugin DLL
1. Open `plugin/LiftoffUnityBridge.sln` in Visual Studio 2022 (v143 toolset).
2. Set **x64 / Release**.
3. Build → `LiftoffUnityBridge.dll` appears in `plugin/x64/Release/`.

Copy the DLL into `Assets/Plugins/x86_64/`.

## Unity Setup
1. Copy `Assets/` into your project:
   - `Assets/Scripts/LiftoffWindows.cs`
   - `Assets/Scripts/LiftoffSample.cs`
   - `Assets/Plugins/x86_64/` (drop the built DLLs here)
2. In **Player Settings**, target **Windows x86_64**.
3. Add `LiftoffSample` to a scene, fill `appId` and `placement`, and press Play (Windows Editor) or build a Windows player.

## WebView2 Runtime
The Windows SDK renders via Edge WebView2. Ship the **Evergreen Runtime** with your installer or require it on end-user machines. Keep `WebView2Loader.dll` next to your plugin DLL.

## Cross-Platform
- On **Windows** (Standalone or Editor): the wrapper P/Invokes `LiftoffUnityBridge.dll`.
- On **iOS / Android / others**: the wrapper compiles and provides **no-op** methods so your game code builds cleanly.

## Notes
- Ensure you also configure **app-ads.txt** for your domain.
- Validate your **App ID** and **Placement** in the Liftoff dashboard.
- You can further expand callbacks and configuration according to your SDK version.
