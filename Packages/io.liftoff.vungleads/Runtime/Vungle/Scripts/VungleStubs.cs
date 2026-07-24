
namespace VungleAds
{
    // Stub implementations for unsupported platforms (not iOS or Android).
    // These no-op methods allow the plugin to compile on targets like
    // macOS, Windows, Linux, and WebGL without runtime functionality.

    #if !UNITY_IOS && !UNITY_ANDROID

    public partial class VungleBannerView
    {
        public VungleBannerView(string placementId, VungleBannerSize adSize)
        {
            this.placementId = placementId;
            this.adSize = adSize;
        }

        public VungleBannerView(string placementId, int width)
        {
            this.placementId = placementId;
            this.adSize = VungleBannerSize.FlexibleHeight;
            this.customWidth = width;
        }

        public VungleBannerView(string placementId, int width, int height)
        {
            this.placementId = placementId;
            this.adSize = VungleBannerSize.FixedSize;
            this.customWidth = width;
            this.customHeight = height;
        }

        public void Load() { }
        private void AttachNative(int x, int y, int width, int height) { }
        private void DetachNative() { }
        private void DestroyNative() { }
    }

    public partial class VungleInterstitial
    {
        public VungleInterstitial(string placementId)
        {
            this.placementId = placementId;
        }

        public bool CanPlay() { return false; }
        public void Load() { }
        public void LoadWithCsbData(VungleCSBData csbData) { }
        public void Show() { }
        public double GetWinningPrice() { return 0; }
        public void SendWinURL() { }
        public void SendLossURL() { }
    }

    public partial class VungleRewarded
    {
        public VungleRewarded(string placementId)
        {
            this.placementId = placementId;
        }

        public bool CanPlay() { return false; }
        public void Load() { }
        public void LoadWithCsbData(VungleCSBData csbData) { }
        public void Show() { }
        public double GetWinningPrice() { return 0; }
        public void SendWinURL() { }
        public void SendLossURL() { }
    }

    public partial class VungleNative
    {
        public VungleNative(string placementId)
        {
            this.placementId = placementId;
        }

        public bool CanPlay() { return false; }
        public void Load() { }
        private void AttachNative(int x, int y, int width, int height,
            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
            int[] clickableRects) { }
        private void DetachNative() { }
        private void DestroyNative() { }
    }

    #endif
}
