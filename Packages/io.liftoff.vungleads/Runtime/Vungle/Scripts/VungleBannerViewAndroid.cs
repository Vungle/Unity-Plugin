using System;
using System.Collections.Generic;
using UnityEngine;

namespace VungleAds
{
    #if UNITY_ANDROID

    public partial class VungleBannerView : AndroidJavaProxy
    {
        private AndroidJavaObject bannerViewObject;

        public VungleBannerView(string placementId, VungleBannerSize adSize) : base("com.vungle.androidplugin.IVungleBannerViewCallbackReceiver")
        {
            this.placementId = placementId;
            this.adSize = adSize;
            this.customWidth = 0;
            this.customHeight = 0;
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginBannerView");
            bannerViewObject = vungleClass.CallStatic<AndroidJavaObject>("createInstance", this.placementId, (int)this.adSize, 0, 0, this);
        }

        public VungleBannerView(string placementId, int width) : base("com.vungle.androidplugin.IVungleBannerViewCallbackReceiver")
        {
            this.placementId = placementId;
            this.adSize = VungleBannerSize.FlexibleHeight;
            this.customWidth = width;
            this.customHeight = 0;
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginBannerView");
            bannerViewObject = vungleClass.CallStatic<AndroidJavaObject>("createInstance", this.placementId, (int)this.adSize, this.customWidth, 0, this);
        }

        public VungleBannerView(string placementId, int width, int height) : base("com.vungle.androidplugin.IVungleBannerViewCallbackReceiver")
        {
            this.placementId = placementId;
            this.adSize = VungleBannerSize.FixedSize;
            this.customWidth = width;
            this.customHeight = height;
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginBannerView");
            bannerViewObject = vungleClass.CallStatic<AndroidJavaObject>("createInstance", this.placementId, (int)this.adSize, this.customWidth, this.customHeight, this);
        }

        ~VungleBannerView()
        {
            bannerViewObject?.Dispose();
            bannerViewObject = null;
        }

        public void Load()
        {
            if (isDestroyed || bannerViewObject == null) return;
            bannerViewObject.Call("loadAd");
        }

        private void AttachNative(int x, int y, int width, int height)
        {
            if (bannerViewObject == null) return;
            bannerViewObject.Call("attach", x, y, width, height);
        }

        private void DetachNative()
        {
            bannerViewObject?.Call("detach");
        }

        private void DestroyNative()
        {
            bannerViewObject?.Call("destroy");
            bannerViewObject?.Dispose();
            bannerViewObject = null;
        }

        #region BannerView Callbacks

        public void BannerViewLoadedCallback() => VungleThreadDispatcher.Enqueue(() => onLoadSuccess?.Invoke());
        public void BannerViewFailedToLoadCallback(String error) => VungleThreadDispatcher.Enqueue(() => onLoadFailed?.Invoke(error));
        public void BannerViewDidPresentCallback() => VungleThreadDispatcher.Enqueue(() => onDidPresent?.Invoke());
        public void BannerViewFailedToPresentCallback(String error) => VungleThreadDispatcher.Enqueue(() => onPresentFailed?.Invoke(error));
        public void BannerViewDidCloseCallback() => VungleThreadDispatcher.Enqueue(() => onDidClose?.Invoke());
        public void BannerViewDidTrackImpressionCallback() => VungleThreadDispatcher.Enqueue(() => onImpression?.Invoke());
        public void BannerViewDidClickCallback() => VungleThreadDispatcher.Enqueue(() => onClick?.Invoke());
        public void BannerViewWillLeaveApplicationCallback() => VungleThreadDispatcher.Enqueue(() => onWillLeaveApplication?.Invoke());

        #endregion
    }

    #endif
}
