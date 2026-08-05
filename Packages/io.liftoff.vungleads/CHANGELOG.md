# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [7.0.2] - 2026-07-24

### Fixed
- Native ads: calling `Attach` before the ad loaded left the bridge believing
  it was registered with the SDK, so the attach after a successful load
  silently skipped registration and the media never rendered. `Attach` now
  fails fast with `onPresentFailed` when the ad is not ready, and a failed
  SDK registration (not loaded, expired, ...) clears the bridge state so the
  next attach re-registers from scratch.
- Native ads (iOS): touches on the native ad container no longer leak into
  Unity with corrupted coordinates. UIKit forwards unhandled touches up the
  responder chain into Unity's view, and Unity normalizes positions by the
  originating view's bounds — so taps on non-clickable regions of the ad
  container fired phantom clicks on unrelated Unity UI (e.g. a top-of-screen
  button). The container now swallows touches; SDK click gestures are
  unaffected.
- Native ads (Android): fixed a crash when re-attaching with a changed
  clickable set. The SDK fires onAdEnd synchronously from unregisterView and
  silently ignores re-registration on the same root view, so the bridge now
  quietly unregisters (listener detached) and rebuilds fresh views before
  re-registering. Previously this crashed with a null rootView — and when it
  didn't crash, the changed clickable set was silently not applied.
- Native ads (Android): fixed a crash when destroying an attached ad (e.g.
  loading a replacement ad). The SDK fires `onAdEnd` synchronously from inside
  `unregisterView`, and the close handler nulled fields the destroy
  continuation still used (NPE on `setAdListener`). Teardown now detaches the
  listener before unregistering, the close handler only hides views, and all
  SDK callbacks and public bridge methods are null-guarded.
- Native ads: added `VungleNative.Destroy()` for end-of-life cleanup — it
  unregisters the ad from the SDK (`unregisterView`, releasing the rendered
  media content, ad options view, and click listeners) and tears down the
  native views and bridge references. Previously nothing ever unregistered,
  leaking the ad views on screen when leaving a scene or loading a
  replacement ad (on iOS the plugin object was also retained forever by the
  internal references map). `Detach` stays lightweight — it only hides the
  ad and a later `Attach` re-shows it. The C# finalizer now performs the
  same native teardown as a safety net if `Destroy` was never called.

### Added
- Native ads: `VungleNative.Attach` overload taking a separate media rect and a
  list of clickable rects. Each rect becomes a clickable region registered with
  the SDK, so publishers choose exactly which app-rendered elements (title,
  icon, CTA, ...) count as ad clicks. Note the SDK only defaults the media view
  to clickable when no list is given — include a media rect to keep it
  clickable (or omit it to make the media view not clickable).
- Native ads: `VungleNative.Attach(RectTransform slot, RectTransform mediaSlot,
  RectTransform[] clickableViews)` — RectTransform-based attach that captures
  the slot's screen position at call time, so publishers don't have to compute
  screen rects by hand.

### Removed
- Banner: the `track` option of `Attach(RectTransform)` (per-frame position
  tracking for scrolling layouts) and the scroll-test sample scene. The native
  overlay renders above all Unity UI and cannot be clipped by Unity viewports,
  which produces unavoidable visual artifacts in scrolling feeds. Attach is
  now position-at-call-time only; re-Attach after layout changes.
- Android dependency now resolves as a version range `[7.7,8.0)` (matching the
  iOS pod's `~> 7.7`) so publishers pick up new 7.x SDK releases automatically.

## [7.0.1] - 2026-06-02

### Fixed
- Unity Asset Store listing: restored compatibility with the other major Unity
  versions. The version selection was deselected during the 7.0.0 deployment
  push, which limited the store listing's availability. No code changes.

## [7.0.0] - 2026-05-05

Tested with Vungle Ads SDK Android 7.7.4, iOS 7.7.2.

### Added
- Initial UPM package release (`io.liftoff.vungleads`); minimum supported Unity version 6000.0.66f2
- Android support via Java Android plugin sub-project bridge
- iOS support via Objective-C plugin bridge
- `VungleSdk` — SDK initialisation with `Init(appId)`, success/failure callbacks
- `VungleInterstitial` — full-screen interstitial ad with load/show lifecycle callbacks
- `VungleRewarded` — rewarded ad with load/show/reward lifecycle callbacks
- `VungleBannerView` — banner ad supporting standard sizes (Banner, BannerShort, Leaderboard, MREC, FlexibleHeight, FixedSize) and `RectTransform`-tracked positioning
- `VungleNative` — native ad with asset delivery (`AdTitle`, `AdBody`, `AdCallToAction`, `AdStarRating`, `AdIconUrl`)
- `VungleFpd` — first-party data API
- Editor stub (`VungleUnityEditor`) for in-editor play mode
- `VungleiOSBuildHelper` — post-build Xcode project configurator (SKAdNetwork IDs injected into Info.plist)
- `VungleDependencies.xml` for External Dependency Manager (EDM4U) resolution
- Assembly definitions for Runtime and Editor assemblies
