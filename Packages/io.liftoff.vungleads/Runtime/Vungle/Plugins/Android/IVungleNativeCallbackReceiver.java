package com.vungle.androidplugin;

public interface IVungleNativeCallbackReceiver {
    void NativeLoadedCallback();
    void NativeFailedToLoadCallback(String error);
    void NativeDidPresentCallback();
    void NativeFailedToPresentCallback(String error);
    void NativeDidCloseCallback();
    void NativeDidTrackImpressionCallback();
    void NativeDidClickCallback();
    void NativeWillLeaveApplicationCallback();
    void NativeAdDataCallback(String title, String body, String ctaText, double rating, String iconUrl);
}
