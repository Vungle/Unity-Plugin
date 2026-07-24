# Liftoff Ads — Unity Plugin

Unity plugin for the Liftoff/Vungle Ads SDK, supporting **Android** and **iOS**. Provides interstitial, rewarded, banner, and native ad formats via a unified C# API.

| Platform | Native SDK |
|----------|-----------|
| Android  | Vungle Ads 7.7.4 (`com.vungle:vungle-ads`) |
| iOS      | VungleAds 7.7.2 (CocoaPod) |

---

## Requirements

- Unity 6000.0.66f2 or newer
- External Dependency Manager for Unity (EDM4U) — resolves native Android/iOS SDK dependencies automatically

---

## Quick Start

### 1. Initialise the SDK

Always register your callbacks **before** calling `Init`. The SDK initialises asynchronously; load ads only after `onInitializeSuccessEvent` fires.

```csharp
using VungleAds;

VungleSdk.onInitializeSuccessEvent += OnSdkInitialized;
VungleSdk.onInitializeFailedEvent  += err => Debug.LogError("Vungle init failed: " + err);
VungleSdk.Init("YOUR_APP_ID");

void OnSdkInitialized()
{
    Debug.Log("Vungle SDK ready");
    // Safe to load ads now
}
```

---

## Ad Formats

### Interstitial

Full-screen ads that display between natural breaks in your app flow.

```csharp
using VungleAds;

VungleInterstitial interstitialAd;

void LoadInterstitial()
{
    interstitialAd = new VungleInterstitial("YOUR_PLACEMENT_ID");

    interstitialAd.onLoadSuccess    = () => Debug.Log("Interstitial loaded — ready to show");
    interstitialAd.onLoadFailed     = err  => Debug.LogWarning("Load failed: " + err);
    interstitialAd.onWillPresent    = ()   => Debug.Log("Interstitial about to appear");
    interstitialAd.onDidPresent     = ()   => Debug.Log("Interstitial visible");
    interstitialAd.onPresentFailed  = err  => Debug.LogWarning("Present failed: " + err);
    interstitialAd.onImpression     = ()   => Debug.Log("Impression recorded");
    interstitialAd.onClick          = ()   => Debug.Log("Ad clicked");
    interstitialAd.onWillLeaveApplication = () => Debug.Log("Leaving app via ad");
    interstitialAd.onWillClose      = ()   => Debug.Log("Interstitial closing");
    interstitialAd.onDidClose       = ()   =>
    {
        Debug.Log("Interstitial closed");
        interstitialAd = null;   // Object is invalid after close — always null out
    };

    interstitialAd.Load();
}

// Call after onLoadSuccess has fired
void ShowInterstitial()
{
    interstitialAd?.Show();
}
```

#### Interstitial callback reference

| Callback | When it fires | Platform |
|----------|--------------|----------|
| `onLoadSuccess` | Ad loaded and ready to show | Both |
| `onLoadFailed(error)` | Load request failed | Both |
| `onWillPresent` | Ad is about to become visible | iOS only |
| `onDidPresent` | Ad is now on screen | Both |
| `onPresentFailed(error)` | `Show()` was called but the ad could not display | Both |
| `onImpression` | Impression tracker fired | Both |
| `onClick` | User tapped the ad | Both |
| `onWillLeaveApplication` | Ad is redirecting the user to an external URL | Both |
| `onWillClose` | Ad is beginning to dismiss | iOS only |
| `onDidClose` | Ad has fully dismissed — object is now invalid | Both |

---

### Rewarded

Rewarded ads let users opt in to watch an ad in exchange for an in-app reward.

```csharp
using VungleAds;

VungleRewarded rewardedAd;

void LoadRewarded()
{
    rewardedAd = new VungleRewarded("YOUR_PLACEMENT_ID");

    rewardedAd.onLoadSuccess    = ()  => Debug.Log("Rewarded loaded — ready to show");
    rewardedAd.onLoadFailed     = err => Debug.LogWarning("Load failed: " + err);
    rewardedAd.onWillPresent    = ()  => Debug.Log("Rewarded about to appear");
    rewardedAd.onDidPresent     = ()  => Debug.Log("Rewarded visible");
    rewardedAd.onPresentFailed  = err => Debug.LogWarning("Present failed: " + err);
    rewardedAd.onImpression     = ()  => Debug.Log("Impression recorded");
    rewardedAd.onClick          = ()  => Debug.Log("Ad clicked");
    rewardedAd.onWillLeaveApplication = () => Debug.Log("Leaving app via ad");
    rewardedAd.onWillClose      = ()  => Debug.Log("Rewarded closing");
    rewardedAd.onDidClose       = ()  =>
    {
        Debug.Log("Rewarded closed");
        rewardedAd = null;   // Object is invalid after close — always null out
    };
    rewardedAd.onDidRewardUser  = ()  => Debug.Log("User earned reward");

    rewardedAd.Load();
}

// Call after onLoadSuccess has fired
void ShowRewarded()
{
    rewardedAd?.Show();
}
```

#### Rewarded callback reference

| Callback | When it fires | Platform |
|----------|--------------|----------|
| `onLoadSuccess` | Ad loaded and ready to show | Both |
| `onLoadFailed(error)` | Load request failed | Both |
| `onWillPresent` | Ad is about to become visible | iOS only |
| `onDidPresent` | Ad is now on screen | Both |
| `onPresentFailed(error)` | `Show()` failed | Both |
| `onImpression` | Impression tracker fired | Both |
| `onClick` | User tapped the ad | Both |
| `onWillLeaveApplication` | Redirecting to external URL | Both |
| `onWillClose` | Ad is beginning to dismiss | iOS only |
| `onDidClose` | Ad fully dismissed — object invalid | Both |
| `onDidRewardUser` | User completed the rewarded view | Both |

---

### Banner

Banner ads overlay your game UI and stay visible until explicitly detached or destroyed.

```csharp
using VungleAds;

VungleBannerView bannerAd;

void LoadBanner()
{
    // Standard 320x50 banner
    bannerAd = new VungleBannerView("YOUR_PLACEMENT_ID", VungleBannerSize.Banner);

    // Alternative constructors:
    // bannerAd = new VungleBannerView("YOUR_PLACEMENT_ID", customWidth);               // FlexibleHeight
    // bannerAd = new VungleBannerView("YOUR_PLACEMENT_ID", customWidth, customHeight); // FixedSize

    bannerAd.onLoadSuccess   = ()  =>
    {
        Debug.Log("Banner loaded");
        PositionBanner();
    };
    bannerAd.onLoadFailed    = err => Debug.LogWarning("Load failed: " + err);
    bannerAd.onWillPresent   = ()  => Debug.Log("Banner about to appear");   // iOS only
    bannerAd.onDidPresent    = ()  => Debug.Log("Banner visible");
    bannerAd.onPresentFailed = err => Debug.LogWarning("Present failed: " + err);
    bannerAd.onImpression    = ()  => Debug.Log("Impression recorded");
    bannerAd.onClick         = ()  => Debug.Log("Banner clicked");
    bannerAd.onWillLeaveApplication = () => Debug.Log("Leaving app via ad");
    bannerAd.onWillClose     = ()  => Debug.Log("Banner closing");           // iOS only
    bannerAd.onDidClose      = ()  => { Debug.Log("Banner closed"); bannerAd = null; };

    bannerAd.Load();
}

void PositionBanner()
{
    // Option A: absolute pixel position (y = 0 is top of screen)
    int bannerHeightPx = Mathf.RoundToInt(50 * (Screen.dpi > 0 ? Screen.dpi / 160f : 2f));
    int x = (Screen.width - Mathf.RoundToInt(320 * (Screen.dpi > 0 ? Screen.dpi / 160f : 2f))) / 2;
    int y = Screen.height - bannerHeightPx;   // bottom of screen
    bannerAd.Attach(x, y, bannerPxW, bannerHeightPx);

    // Option B: snap to a RectTransform's current position
    // bannerAd.Attach(myRectTransform);
}

void DestroyBanner()
{
    bannerAd?.Destroy();
    bannerAd = null;
}
```

#### Banner sizes

| Enum | Dimensions | Constructor |
|------|-----------|-------------|
| `Banner` | 320 × 50 | `new VungleBannerView(id, VungleBannerSize.Banner)` |
| `BannerShort` | 300 × 50 | `new VungleBannerView(id, VungleBannerSize.BannerShort)` |
| `BannerLeaderboard` | 728 × 90 | `new VungleBannerView(id, VungleBannerSize.BannerLeaderboard)` |
| `Mrec` | 300 × 250 | `new VungleBannerView(id, VungleBannerSize.Mrec)` |
| `FlexibleHeight` | Publisher-defined width, creative-determined height | `new VungleBannerView(id, width)` |
| `FixedSize` | Custom width and height | `new VungleBannerView(id, width, height)` |

#### Banner positioning

- `Attach(x, y)` — pixel coordinates; **y = 0 is the top of the screen**.
- `Attach(x, y, width, height)` — same origin, explicit size.
- `Attach(RectTransform slot)` — snaps the banner to the slot's current position. The position is captured at call time; call `Attach` again if your layout moves (there is no per-frame tracking — the native banner view renders above all Unity UI and cannot be clipped by Unity viewports, so scrolling placements are not supported).
- All coordinates are **screen pixels**, not Unity units or dp/pt. Convert using `Screen.dpi / 160f` as the density factor.
- Call `Detach()` to hide the banner without releasing resources. Call `Destroy()` to release all native resources.

#### Banner callback reference

| Callback | When it fires | Platform |
|----------|--------------|----------|
| `onLoadSuccess` | Ad loaded and ready to attach | Both |
| `onLoadFailed(error)` | Load request failed | Both |
| `onWillPresent` | Ad is about to become visible | iOS only |
| `onDidPresent` | Ad is now on screen | Both |
| `onPresentFailed(error)` | `Attach()` failed | Both |
| `onImpression` | Impression tracker fired | Both |
| `onClick` | User tapped the ad | Both |
| `onWillLeaveApplication` | Ad is redirecting the user to an external URL | Both |
| `onWillClose` | Ad is beginning to dismiss | iOS only |
| `onDidClose` | Ad has dismissed — call `Destroy()` to release resources | Both |

---

### Native

Native ads give you full control over ad layout — you render the creative elements yourself using the ad data provided by the SDK. The SDK renders only the main media (image/video) into a native view that you position over your layout; you choose which of your app-rendered elements (icon, title, body, CTA, ...) count as ad clicks via **clickable views**.

```csharp
using VungleAds;

VungleNative nativeAd;

void LoadNative()
{
    nativeAd = new VungleNative("YOUR_PLACEMENT_ID");

    nativeAd.onAdDataReceived = (title, body, cta, rating, iconUrl) =>
    {
        // Fires shortly AFTER onLoadSuccess — populate your UI here, not in
        // onLoadSuccess (the ad properties are still empty at that point)
        titleLabel.text  = title;
        bodyLabel.text   = body;
        ctaButton.text   = cta;
        starRating.value = (float)rating;
        // iconUrl is a local file path — load it with UnityWebRequest or File.ReadAllBytes
    };
    nativeAd.onLoadSuccess   = ()  =>
    {
        Debug.Log("Native loaded");
        AttachNativeAd();
    };
    nativeAd.onLoadFailed    = err => Debug.LogWarning("Load failed: " + err);
    nativeAd.onDidPresent    = ()  => Debug.Log("Native media visible");
    nativeAd.onPresentFailed = err => Debug.LogWarning("Present failed: " + err);
    nativeAd.onImpression    = ()  => Debug.Log("Impression recorded");
    nativeAd.onClick         = ()  => Debug.Log("Native clicked");
    nativeAd.onWillLeaveApplication = () => Debug.Log("Leaving app via ad");
    nativeAd.onDidClose      = ()  => { Debug.Log("Native closed"); nativeAd = null; };

    nativeAd.Load();
}

void AttachNativeAd()
{
    if (nativeAd == null) return;

    // Attach over your ad card: the container covers cardSlot, the SDK's
    // media view is laid out over mediaSlot, and each RectTransform in
    // clickableViews becomes a clickable region (taps there count as ad
    // clicks). IMPORTANT: when a clickable list is provided, ONLY those
    // regions are clickable — include mediaSlot to keep the media clickable.
    nativeAd.Attach(cardSlot, mediaSlot,
        clickableViews: new[] { mediaSlot, iconSlot, ctaSlot });

    // Coordinate-based overloads are also available (y = 0 at top,
    // units are screen pixels): Attach(x, y, w, h) attaches the media
    // only, and Attach(x, y, w, h, mediaX, mediaY, mediaW, mediaH,
    // RectInt[] clickableRects) gives full control.
}

void HideNative()
{
    nativeAd?.Detach();   // hides the ad; a later Attach shows it again
}

void ReleaseNative()
{
    nativeAd?.Destroy();  // unregisters from the SDK and releases the ad
    nativeAd = null;      // views — call on scene exit or before replacing
}
```

Positions are captured at `Attach` time; call `Attach` again if your layout
moves. Calling `Attach` before `onLoadSuccess` fires `onPresentFailed`.

**Rendering ad text is your responsibility** — including international glyph
coverage. Ad titles and CTAs can be in any language (CJK, Cyrillic, Arabic,
...), and TextMeshPro's default font only covers Latin. Register fallback
fonts (e.g. dynamic font assets built from OS fonts, as the package sample's
`UIHelper` does) or non-Latin ad text will render as missing-glyph boxes.

#### Native callback reference

| Callback | When it fires | Platform |
|----------|--------------|----------|
| `onAdDataReceived(title, body, cta, rating, iconUrl)` | Ad creative data ready — fires shortly after `onLoadSuccess` | Both |
| `onLoadSuccess` | Ad loaded and ready to attach (ad data properties not yet populated) | Both |
| `onLoadFailed(error)` | Load request failed | Both |
| `onDidPresent` | Native media view is now on screen | Android only |
| `onPresentFailed(error)` | `Attach()` failed (including `Attach` before load) | Both |
| `onImpression` | Impression tracker fired | Both |
| `onClick` | User tapped the ad or a clickable region | Both |
| `onWillLeaveApplication` | Ad is redirecting the user to an external URL | Android only |
| `onDidClose` | Ad dismissed by the SDK — call `Destroy()` to release | Android only |

The iOS SDK's native ad delegate has no present/close/leave-application
callbacks, so those three events fire on Android only. Do not gate cleanup on
`onDidClose` — call `Destroy()` yourself when the ad is no longer needed.

#### Native ad properties (populated when `onAdDataReceived` fires)

| Property | Type | Description |
|----------|------|-------------|
| `AdTitle` | `string` | Advertiser name or headline |
| `AdBody` | `string` | Ad body copy |
| `AdCallToAction` | `string` | CTA button label (e.g. "Install Now") |
| `AdStarRating` | `double` | Star rating (0–5), 0 if not available |
| `AdIconUrl` | `string` | Local file path to icon image |

---

## Banner Attach coordinates

Both platforms use the same coordinate space for `Attach()`:

- Origin is the **top-left corner of the screen**.
- Units are **screen pixels** (not dp, pt, or Unity world units).
- `y = 0` is the top edge; `y = Screen.height - bannerHeightPx` is the bottom edge.

To convert standard banner dp dimensions to pixels:

```csharp
float density     = Screen.dpi > 0 ? Screen.dpi / 160f : 2f;
int   bannerPxW   = Mathf.RoundToInt(320 * density);  // e.g. for Banner 320dp
int   bannerPxH   = Mathf.RoundToInt(50  * density);
int   x           = (Screen.width - bannerPxW) / 2;   // horizontally centred
int   yBottom     = Screen.height - bannerPxH;         // anchored to bottom
bannerAd.Attach(x, yBottom, bannerPxW, bannerPxH);
```

## Editor behaviour

All SDK calls are no-ops in the Unity Editor and callbacks are never invoked. Guard ad code with `#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR` or test on device.

---

## Best Practices

**Wait for `onLoadSuccess` before calling `Show()` or `Attach()`**
Only call `Show()` (or `Attach()` for banners and native) after `onLoadSuccess` has fired. Enable your show button or trigger in that callback.

**Assign all callbacks before calling `Load()`**
Callbacks assigned after `Load()` may miss early events on fast connections.

**Destroy ads you no longer need**
Banners and native ads hold native view resources. Call `Destroy()` when the ad is no longer required (e.g. on scene unload, or before loading a replacement) — `Detach()` only hides the ad and keeps its resources for re-attaching.

**One ad object lifecycle**
Do not reuse the same `VungleInterstitial` / `VungleRewarded` / `VungleNative` object across multiple load cycles. Create a new instance each time.

---

## Project Structure

```
Packages/io.liftoff.vungleads/
├── Runtime/
│   └── Vungle/
│       ├── Scripts/            C# API (VungleSdk, VungleInterstitial, VungleRewarded,
│       │                                VungleBannerView, VungleNative, VungleFpd, VungleCSBData)
│       └── Plugins/
│           ├── Android/        Java bridge (VunglePluginInterstitialAd, etc.)
│           └── iOS/            Objective-C bridge (.h / .m)
├── Editor/
│   └── Vungle/
│       ├── VungleDependencies.xml   EDM4U dependency manifest
│       └── VungleiOSBuildHelper.cs  Post-process: injects SKAdNetwork IDs into Info.plist
├── Samples~/
│   └── Scripts/                Working example scenes for all ad formats
├── Documentation/
├── CHANGELOG.md
├── LICENSE.md
└── package.json
```

---

## License

See [LICENSE.md](LICENSE.md) — Liftoff/Vungle SDK License and Publisher Terms.
