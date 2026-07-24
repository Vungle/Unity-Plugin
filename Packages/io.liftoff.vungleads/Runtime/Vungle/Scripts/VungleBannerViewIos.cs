using System;
using System.Runtime.InteropServices;
using AOT;

namespace VungleAds
{
    #if UNITY_IOS

    public partial class VungleBannerView
    {
        private IntPtr bannerViewPtr = IntPtr.Zero;
        private IntPtr unityBannerViewPtr = IntPtr.Zero;

        internal delegate void VungleBannerViewAdLoadedCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdFailedToLoadCallback(IntPtr bannerView, string error);
        internal delegate void VungleBannerViewAdWillPresentCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdDidPresentCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdFailedToPresentCallback(IntPtr bannerView, string error);
        internal delegate void VungleBannerViewAdWillCloseCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdDidCloseCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdDidTrackImpressionCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdDidClickCallback(IntPtr bannerView);
        internal delegate void VungleBannerViewAdWillLeaveApplicationCallback(IntPtr bannerView);

        public VungleBannerView(string placementId, VungleBannerSize adSize)
        {
            this.placementId = placementId;
            this.adSize = adSize;
            this.customWidth = 0;
            this.customHeight = 0;
            this.BuildBannerView();
        }

        public VungleBannerView(string placementId, int width)
        {
            this.placementId = placementId;
            this.adSize = VungleBannerSize.FlexibleHeight;
            this.customWidth = width;
            this.customHeight = 0;
            this.BuildBannerView();
        }

        public VungleBannerView(string placementId, int width, int height)
        {
            this.placementId = placementId;
            this.adSize = VungleBannerSize.FixedSize;
            this.customWidth = width;
            this.customHeight = height;
            this.BuildBannerView();
        }

        public void BuildBannerView()
        {
            this.unityBannerViewPtr = (IntPtr)GCHandle.Alloc(this);
            this.bannerViewPtr = CreateVungleBannerView(this.unityBannerViewPtr, this.placementId, (int)this.adSize, this.customWidth, this.customHeight);
            SetVungleBannerViewCallbacks(this.bannerViewPtr,
                BannerViewLoadedCallback,
                BannerViewFailedToLoadCallback,
                BannerViewWillPresentCallback,
                BannerViewDidPresentCallback,
                BannerViewFailedToPresentCallback,
                BannerViewWillCloseCallback,
                BannerViewDidCloseCallback,
                BannerViewDidTrackImpressionCallback,
                BannerViewDidClickCallback,
                BannerViewWillLeaveApplicationCallback);
        }

        ~VungleBannerView()
        {
            if (unityBannerViewPtr != IntPtr.Zero)
            {
                ((GCHandle)unityBannerViewPtr).Free();
                unityBannerViewPtr = IntPtr.Zero;
            }
        }

        public void Load()
        {
            if (isDestroyed || bannerViewPtr == IntPtr.Zero) return;
            LoadVungleBannerView(this.bannerViewPtr);
        }

        private void AttachNative(int x, int y, int width, int height)
        {
            if (bannerViewPtr == IntPtr.Zero) return;
            AttachVungleBannerView(this.bannerViewPtr, x, y, width, height);
        }

        private void DetachNative()
        {
            if (bannerViewPtr == IntPtr.Zero) return;
            DetachVungleBannerView(this.bannerViewPtr);
        }

        private void DestroyNative()
        {
            DestroyVungleBannerView(this.bannerViewPtr);
            bannerViewPtr = IntPtr.Zero;
            if (unityBannerViewPtr != IntPtr.Zero)
            {
                ((GCHandle)unityBannerViewPtr).Free();
                unityBannerViewPtr = IntPtr.Zero;
            }
        }

        #region BannerView Callbacks

        [MonoPInvokeCallback(typeof(VungleBannerViewAdLoadedCallback))]
        private static void BannerViewLoadedCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onLoadSuccess?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdFailedToLoadCallback))]
        private static void BannerViewFailedToLoadCallback(IntPtr bannerView, string error)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onLoadFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdWillPresentCallback))]
        private static void BannerViewWillPresentCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onWillPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdDidPresentCallback))]
        private static void BannerViewDidPresentCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onDidPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdFailedToPresentCallback))]
        private static void BannerViewFailedToPresentCallback(IntPtr bannerView, string error)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onPresentFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdWillCloseCallback))]
        private static void BannerViewWillCloseCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onWillClose?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdDidCloseCallback))]
        private static void BannerViewDidCloseCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onDidClose?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdDidTrackImpressionCallback))]
        private static void BannerViewDidTrackImpressionCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onImpression?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdDidClickCallback))]
        private static void BannerViewDidClickCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onClick?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleBannerViewAdWillLeaveApplicationCallback))]
        private static void BannerViewWillLeaveApplicationCallback(IntPtr bannerView)
        {
            VungleBannerView client = IntPtrToBannerView(bannerView);
            if (client == null) return;
            client.onWillLeaveApplication?.Invoke();
        }

        private static VungleBannerView IntPtrToBannerView(IntPtr bannerView)
        {
            GCHandle handle = (GCHandle)bannerView;
            return handle.Target as VungleBannerView;
        }

        #endregion

        #region BannerView Externs

        [DllImport("__Internal")]
        internal static extern IntPtr CreateVungleBannerView(IntPtr unityRef, string placementId, int adSizeType, int width, int height);

        [DllImport("__Internal")]
        internal static extern void SetVungleBannerViewCallbacks(IntPtr bannerView,
            VungleBannerViewAdLoadedCallback adLoadedCallback,
            VungleBannerViewAdFailedToLoadCallback adFailedToLoadCallback,
            VungleBannerViewAdWillPresentCallback adWillPresentCallback,
            VungleBannerViewAdDidPresentCallback adDidPresentCallback,
            VungleBannerViewAdFailedToPresentCallback adFailedToPresentCallback,
            VungleBannerViewAdWillCloseCallback adWillCloseCallback,
            VungleBannerViewAdDidCloseCallback adDidCloseCallback,
            VungleBannerViewAdDidTrackImpressionCallback adDidTrackImpressionCallback,
            VungleBannerViewAdDidClickCallback adDidClickCallback,
            VungleBannerViewAdWillLeaveApplicationCallback adWillLeaveApplicationCallback);

        [DllImport("__Internal")]
        internal static extern void LoadVungleBannerView(IntPtr bannerView);

        [DllImport("__Internal")]
        internal static extern void AttachVungleBannerView(IntPtr bannerView, int x, int y, int width, int height);

        [DllImport("__Internal")]
        internal static extern void DetachVungleBannerView(IntPtr bannerView);

        [DllImport("__Internal")]
        internal static extern void DestroyVungleBannerView(IntPtr bannerView);

        #endregion
    }

    #endif
}
