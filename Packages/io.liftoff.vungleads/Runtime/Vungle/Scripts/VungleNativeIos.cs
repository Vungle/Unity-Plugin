using System;
using System.Runtime.InteropServices;
using AOT;

namespace VungleAds
{
    #if UNITY_IOS

    public partial class VungleNative
    {
        private IntPtr nativePtr = IntPtr.Zero;
        private IntPtr unityNativePtr = IntPtr.Zero;
        private bool canPlay = false;

        internal delegate void VungleNativeAdLoadedCallback(IntPtr nativeAd);
        internal delegate void VungleNativeAdFailedToLoadCallback(IntPtr nativeAd, string error);
        internal delegate void VungleNativeAdDidPresentCallback(IntPtr nativeAd);
        internal delegate void VungleNativeAdFailedToPresentCallback(IntPtr nativeAd, string error);
        internal delegate void VungleNativeAdDidCloseCallback(IntPtr nativeAd);
        internal delegate void VungleNativeAdDidTrackImpressionCallback(IntPtr nativeAd);
        internal delegate void VungleNativeAdDidClickCallback(IntPtr nativeAd);
        internal delegate void VungleNativeAdWillLeaveApplicationCallback(IntPtr nativeAd);
        internal delegate void VungleNativeAdDataCallback(IntPtr nativeAd, string title, string body, string ctaText, double rating, string iconUrl);

        public VungleNative(string placementId)
        {
            this.placementId = placementId;
            this.BuildNative();
        }

        public void BuildNative()
        {
            this.unityNativePtr = (IntPtr)GCHandle.Alloc(this);
            this.nativePtr = CreateVungleNativeAd(this.unityNativePtr, this.placementId);
            SetVungleNativeAdCallbacks(this.nativePtr,
                NativeLoadedCb,
                NativeFailedToLoadCb,
                NativeDidPresentCb,
                NativeFailedToPresentCb,
                NativeDidCloseCb,
                NativeDidTrackImpressionCb,
                NativeDidClickCb,
                NativeWillLeaveApplicationCb,
                NativeAdDataCb);
        }

        // Deinit safety net: if the publisher never called Destroy, tear down
        // the native side here — DestroyVungleNativeAd also removes the plugin
        // object from the VunglePluginReferences map, which would otherwise
        // retain it (and the ad views) forever
        ~VungleNative()
        {
            try
            {
                DestroyNative();
            }
            catch (Exception)
            {
                // Finalizer thread — never throw
            }
        }

        private void DestroyNative()
        {
            if (nativePtr != IntPtr.Zero)
            {
                DestroyVungleNativeAd(this.nativePtr);
                nativePtr = IntPtr.Zero;
            }
            if (unityNativePtr != IntPtr.Zero)
            {
                ((GCHandle)unityNativePtr).Free();
                unityNativePtr = IntPtr.Zero;
            }
        }

        public bool CanPlay()
        {
            return canPlay && nativePtr != IntPtr.Zero;
        }

        public void Load()
        {
            if (nativePtr == IntPtr.Zero) return;
            LoadVungleNativeAd(this.nativePtr);
        }

        private void AttachNative(int x, int y, int width, int height,
            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
            int[] clickableRects)
        {
            if (nativePtr == IntPtr.Zero) return;
            int count = clickableRects != null ? clickableRects.Length / 4 : 0;
            AttachVungleNativeAdEx(this.nativePtr, x, y, width, height,
                mediaX, mediaY, mediaWidth, mediaHeight,
                clickableRects ?? Array.Empty<int>(), count);
        }

        private void DetachNative()
        {
            if (nativePtr == IntPtr.Zero) return;
            DetachVungleNativeAd(this.nativePtr);
        }

        #region Native Callbacks

        [MonoPInvokeCallback(typeof(VungleNativeAdLoadedCallback))]
        private static void NativeLoadedCb(IntPtr nativeAd)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.canPlay = true;
            client.onLoadSuccess?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdFailedToLoadCallback))]
        private static void NativeFailedToLoadCb(IntPtr nativeAd, string error)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onLoadFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdDidPresentCallback))]
        private static void NativeDidPresentCb(IntPtr nativeAd)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onDidPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdFailedToPresentCallback))]
        private static void NativeFailedToPresentCb(IntPtr nativeAd, string error)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onPresentFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdDidCloseCallback))]
        private static void NativeDidCloseCb(IntPtr nativeAd)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onDidClose?.Invoke();
            client.nativePtr = IntPtr.Zero;
            if (client.unityNativePtr != IntPtr.Zero)
            {
                ((GCHandle)client.unityNativePtr).Free();
                client.unityNativePtr = IntPtr.Zero;
            }
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdDidTrackImpressionCallback))]
        private static void NativeDidTrackImpressionCb(IntPtr nativeAd)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onImpression?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdDidClickCallback))]
        private static void NativeDidClickCb(IntPtr nativeAd)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onClick?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdWillLeaveApplicationCallback))]
        private static void NativeWillLeaveApplicationCb(IntPtr nativeAd)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.onWillLeaveApplication?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleNativeAdDataCallback))]
        private static void NativeAdDataCb(IntPtr nativeAd, string title, string body, string ctaText, double rating, string iconUrl)
        {
            VungleNative client = IntPtrToNative(nativeAd);
            if (client == null) return;
            client.SetAdData(title, body, ctaText, rating, iconUrl);
        }

        private static VungleNative IntPtrToNative(IntPtr nativeAd)
        {
            GCHandle handle = (GCHandle)nativeAd;
            return handle.Target as VungleNative;
        }

        #endregion

        #region Native Externs

        [DllImport("__Internal")]
        internal static extern IntPtr CreateVungleNativeAd(IntPtr unityRef, string placementId);

        [DllImport("__Internal")]
        internal static extern void SetVungleNativeAdCallbacks(IntPtr nativeAd,
            VungleNativeAdLoadedCallback adLoadedCallback,
            VungleNativeAdFailedToLoadCallback adFailedToLoadCallback,
            VungleNativeAdDidPresentCallback adDidPresentCallback,
            VungleNativeAdFailedToPresentCallback adFailedToPresentCallback,
            VungleNativeAdDidCloseCallback adDidCloseCallback,
            VungleNativeAdDidTrackImpressionCallback adDidTrackImpressionCallback,
            VungleNativeAdDidClickCallback adDidClickCallback,
            VungleNativeAdWillLeaveApplicationCallback adWillLeaveApplicationCallback,
            VungleNativeAdDataCallback adDataCallback);

        [DllImport("__Internal")]
        internal static extern void LoadVungleNativeAd(IntPtr nativeAd);

        [DllImport("__Internal")]
        internal static extern void AttachVungleNativeAd(IntPtr nativeAd, int x, int y, int width, int height);

        [DllImport("__Internal")]
        internal static extern void AttachVungleNativeAdEx(IntPtr nativeAd, int x, int y, int width, int height,
            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
            int[] clickableRects, int clickableCount);

        [DllImport("__Internal")]
        internal static extern void DetachVungleNativeAd(IntPtr nativeAd);

        [DllImport("__Internal")]
        internal static extern void DestroyVungleNativeAd(IntPtr nativeAd);

        #endregion
    }

    #endif
}
