package com.vungle.androidplugin;

public interface IVungleBannerViewCallbackReceiver {
    void BannerViewLoadedCallback();
    void BannerViewFailedToLoadCallback(String error);
    void BannerViewDidPresentCallback();
    void BannerViewFailedToPresentCallback(String error);
    void BannerViewDidCloseCallback();
    void BannerViewDidTrackImpressionCallback();
    void BannerViewDidClickCallback();
    void BannerViewWillLeaveApplicationCallback();
}
