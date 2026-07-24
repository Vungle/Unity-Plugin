package com.vungle.androidplugin;

public interface IVungleRewardedCallbackReceiver {
    void RewardedLoadedCallback();
    void RewardedFailedToLoadCallback(String error);
    void RewardedDidPresentCallback();
    void RewardedFailedToPresentCallback(String error);
    void RewardedDidCloseCallback();
    void RewardedDidTrackImpressionCallback();
    void RewardedDidClickCallback();
    void RewardedWillLeaveApplicationCallback();
    void RewardedDidRewardUserCallback();
}
