using System;
using UnityEngine;

namespace VungleAds
{
    #if UNITY_ANDROID

    public partial class VungleNative : AndroidJavaProxy
    {
        private AndroidJavaObject nativeObject;

        public VungleNative(string placementId) : base("com.vungle.androidplugin.IVungleNativeCallbackReceiver")
        {
            this.placementId = placementId;
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginNativeAd");
            nativeObject = vungleClass.CallStatic<AndroidJavaObject>("createInstance", this.placementId, this);
        }

        // Deinit safety net: if the publisher never called Destroy, tear down
        // the native side here so the ad views and SDK registration are
        // released rather than leaked
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
            if (nativeObject == null) return;
            nativeObject.Call("destroy");
            nativeObject.Dispose();
            nativeObject = null;
        }

        public bool CanPlay()
        {
            if (nativeObject == null) return false;
            return nativeObject.Call<bool>("canPlayAd");
        }

        public void Load()
        {
            if (nativeObject == null) return;
            nativeObject.Call("loadAd");
        }

        private void AttachNative(int x, int y, int width, int height,
            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
            int[] clickableRects)
        {
            if (nativeObject == null) return;
            nativeObject.Call("attach", x, y, width, height,
                mediaX, mediaY, mediaWidth, mediaHeight,
                clickableRects ?? Array.Empty<int>());
        }

        private void DetachNative()
        {
            nativeObject?.Call("detach");
        }

        #region Native Callbacks

        public void NativeLoadedCallback() => VungleThreadDispatcher.Enqueue(() => onLoadSuccess?.Invoke());
        public void NativeFailedToLoadCallback(string error) => VungleThreadDispatcher.Enqueue(() => onLoadFailed?.Invoke(error));
        public void NativeDidPresentCallback() => VungleThreadDispatcher.Enqueue(() => onDidPresent?.Invoke());
        public void NativeFailedToPresentCallback(string error) => VungleThreadDispatcher.Enqueue(() => onPresentFailed?.Invoke(error));
        public void NativeDidCloseCallback() => VungleThreadDispatcher.Enqueue(() => onDidClose?.Invoke());
        public void NativeDidTrackImpressionCallback() => VungleThreadDispatcher.Enqueue(() => onImpression?.Invoke());
        public void NativeDidClickCallback() => VungleThreadDispatcher.Enqueue(() => onClick?.Invoke());
        public void NativeWillLeaveApplicationCallback() => VungleThreadDispatcher.Enqueue(() => onWillLeaveApplication?.Invoke());
        public void NativeAdDataCallback(string title, string body, string ctaText, double rating, string iconUrl) => VungleThreadDispatcher.Enqueue(() => SetAdData(title, body, ctaText, rating, iconUrl));

        #endregion
    }

    #endif
}
