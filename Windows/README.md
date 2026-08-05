# Liftoff Monetize — Windows Unity Plugin

Unity integration for the Liftoff/Vungle **Windows SDK v7**. Windows has a different architecture than Android/iOS: Unity's Windows Standalone build has no native-source compile step, so the native↔C# bridge ships as a **prebuilt x64 DLL** (`LiftoffUnityBridge.dll`) built out-of-band with Visual Studio, rather than as source compiled by Unity's per-platform toolchain.

## What's in this folder

| Folder | Contents |
| --- | --- |
| [`LiftoffUnityWindowsPlugin/`](LiftoffUnityWindowsPlugin/README.md) | C++17 bridge source (`plugin/`), Visual Studio solution, and the vendor Windows SDK (`SDK/` — headers, import lib, and `LiftoffSDK.Win32.dll`). |
| `WindowsSDK7SampleApp/` | Unity sample project demonstrating initialization, load, and play on Windows Standalone x86_64. |

The Unity package itself lives at the repo root in [`../Packages/com.liftoff.windows-ads/`](../Packages/com.liftoff.windows-ads/) — the built bridge DLL, vendor SDK DLL, and `WebView2Loader.dll` are shipped in its `Runtime/Plugins/x86_64/`, with C# bindings in `Runtime/Scripts/`.

## How the pieces fit together

1. `LiftoffUnityBridge.cpp` wraps the vendor SDK's C++ API (`LiftoffAds`) behind a flat C ABI (`Liftoff_Initialize`, `Liftoff_LoadAd`, `Liftoff_PlayAd`, …).
2. Building `plugin/LiftoffUnityBridge.sln` (VS 2022, x64) triggers a post-build step that copies the DLL into both `../Packages/com.liftoff.windows-ads/Runtime/Plugins/x86_64/` and `WindowsSDK7SampleApp/Assets/Liftoff/Plugins/x86_64/`.
3. Unity C# code P/Invokes the bridge via `[DllImport("LiftoffUnityBridge")]`; callbacks are marshalled back to the Unity main thread.
4. Ads render via **Microsoft Edge WebView2** — ship the Evergreen Runtime with your installer or require it on end-user machines.

## Building the bridge

See [`LiftoffUnityWindowsPlugin/README.md`](LiftoffUnityWindowsPlugin/README.md) for full build and Unity setup instructions.

## Supported targets

- Windows Standalone **x86_64** (`UNITY_STANDALONE_WIN` / `UNITY_EDITOR_WIN`) only.
- On other platforms the C# wrapper compiles to no-op methods so cross-platform game code builds cleanly.
