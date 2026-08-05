# Liftoff Monetize — Android / iOS Unity Test App

Test app for the **Vungle SDK by Liftoff** Unity plugin (`io.liftoff.vungleads`) on Android and iOS.

The Android/iOS plugin is developed in a private repository and released to the [Unity Asset Store](https://assetstore.unity.com/) as **Vungle SDK by Liftoff**. Unlike Windows, the Android/iOS bridge ships as *source* inside the package — Java glue compiled by Unity's Gradle build and Objective-C glue compiled by the generated Xcode project — with the native VungleAds SDK resolved as a prebuilt dependency (Maven / CocoaPods) via External Dependency Manager (EDM4U).

## What's in this folder

| Folder | Contents |
| --- | --- |
| `VungleSDKSampleApp/` | Unity project with the plugin's sample scenes and scripts (interstitial, rewarded, banner, native, and RectTransform-attached banner formats). |

The project references the package locally from [`../Packages/io.liftoff.vungleads/`](../Packages/io.liftoff.vungleads/) via a `file:` dependency in `Packages/manifest.json`, so it always exercises the package version checked into this repo — the same pattern the Windows sample app uses with the Windows package.

## Getting started

1. Open `VungleSDKSampleApp/` in Unity **6000.0.66f2 or newer** (developed with 6000.3.16f1). Unity regenerates `Library/` and the remaining project files on first open.
2. In **Build Settings**, switch the platform to **Android** or **iOS**.
3. Set your own **Company Name / Product Name / Bundle Identifier** in Player Settings.
4. Replace the app ID and placement IDs in `Assets/Samples/Scripts/VungleConstants.cs` with values from your Liftoff Monetize dashboard.
5. Add the scenes under `Assets/Samples/Scenes/` to Build Settings (start with `LaunchScene`).
   The sample UI is generated at runtime with TextMeshPro; the required **TMP Essential
   Resources** are already included at `Assets/TextMesh Pro/` (if they're ever removed,
   re-import via *Window → TextMeshPro → Import TMP Essential Resources*).
   Ad text can be in any language, so the sample registers dynamic OS-font fallbacks at
   startup for non-Latin glyphs — publishers rendering native-ad text themselves need an
   equivalent font-fallback strategy in their own UI.
6. Build:
   - **Android** — EDM4U resolves `com.vungle:vungle-ads` from Maven during the Gradle build (run *Assets → External Dependency Manager → Android Resolver → Resolve* if needed).
   - **iOS** — EDM4U generates a `Podfile` for the `VungleAds` CocoaPod; open the generated `.xcworkspace` after `pod install`.

## Ad formats covered by the sample scenes

| Scene | Format |
| --- | --- |
| `InterstitialScene` | Interstitial |
| `RewardedScene` | Rewarded |
| `BannerScene`, `DualBanner` | Banner |
| `BannerRectTransformScene` | Banner attached to a RectTransform |
| `NativeScene`, `DualNative` | Native |
