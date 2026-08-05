# Liftoff Monetize (Vungle) Unity Plugin

Unity integration for the Liftoff/Vungle publisher SDKs. Windows and Android/iOS have separate architectures — Windows binds to a prebuilt native bridge DLL, while Android/iOS ship source glue compiled by Unity's per-platform build — so each platform family lives in its own folder with its own documentation.

## Repository layout

| Folder | Description | Docs |
| --- | --- | --- |
| `Packages/com.liftoff.windows-ads/` | UPM package for **Windows** (Standalone x86_64). Ships the prebuilt bridge, vendor SDK, and WebView2 loader DLLs with C# bindings. | [Windows/README.md](Windows/README.md) |
| `Packages/io.liftoff.vungleads/` | UPM package for **Android / iOS** (*Vungle SDK by Liftoff*). Java/Objective-C bridge source plus C# API; native SDK resolved via EDM4U. Developed in a private repository and also released on the Unity Asset Store. | [AndroidIOS/README.md](AndroidIOS/README.md) |
| `Windows/` | Windows-specific sources: the C++ bridge (`LiftoffUnityWindowsPlugin/`, built with Visual Studio 2022) and the `WindowsSDK7SampleApp/` Unity test project. | [Windows/README.md](Windows/README.md) |
| `AndroidIOS/` | Android/iOS test app (`VungleSDKSampleApp/`) built on the package's sample scenes for interstitial, rewarded, banner, and native formats. References `Packages/io.liftoff.vungleads` locally. | [AndroidIOS/README.md](AndroidIOS/README.md) |

## Getting started

- **Windows** — follow [Windows/README.md](Windows/README.md) for the bridge architecture, build steps, and sample app.
- **Android / iOS** — follow [AndroidIOS/README.md](AndroidIOS/README.md) to run the test app; for production integration see the [Liftoff Monetize Unity documentation](https://support.vungle.com/hc/en-us/articles/360003455452).

## License

This plugin is available under a commercial license from LMI Inc., a Liftoff Mobile, Inc. company. See [LICENSE.md](LICENSE.md) for details.
