package com.vungle.androidplugin;

public interface IVungleInterstitialCallbackReceiver {
    void InterstitialLoadedCallback();
    void InterstitialFailedToLoadCallback(String error);
    void InterstitialDidPresentCallback();
    void InterstitialFailedToPresentCallback(String error);
    void InterstitialDidCloseCallback();
    void InterstitialDidTrackImpressionCallback();
    void InterstitialDidClickCallback();
    void InterstitialWillLeaveApplicationCallback();
}
