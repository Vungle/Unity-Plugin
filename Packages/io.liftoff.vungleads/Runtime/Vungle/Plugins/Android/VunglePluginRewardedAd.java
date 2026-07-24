package com.vungle.androidplugin;

import android.content.Context;

import java.util.HashMap;
import java.util.Map;

import com.vungle.ads.AdConfig;
import com.vungle.ads.BaseAd;
import com.vungle.ads.RewardedAd;
import com.vungle.ads.RewardedAdListener;
import com.vungle.ads.VungleCSBData;
import com.vungle.ads.VungleError;

public class VunglePluginRewardedAd implements RewardedAdListener {
    private RewardedAd rewardedAd;
    private IVungleRewardedCallbackReceiver callbackReceiver;

    VunglePluginRewardedAd(String placementId, IVungleRewardedCallbackReceiver receiver) {
        rewardedAd = new RewardedAd(getContext(), placementId, new AdConfig());
        rewardedAd.setAdListener(this);
        callbackReceiver = receiver;
    }

    public static VunglePluginRewardedAd createInstance(String placementId, IVungleRewardedCallbackReceiver receiver) {
        return new VunglePluginRewardedAd(placementId, receiver);
    }

    public static Context getContext() {
        return com.unity3d.player.UnityPlayer.currentActivity.getApplicationContext();
    }

    public boolean canPlay() {
        return rewardedAd.canPlayAd();
    }

    public void loadAd() {
        rewardedAd.load();
    }

    public void loadAdWithCsb(double bidFloor, String auctionId, String creativeId, String adUnitId, boolean isVxWinner, boolean isPriorityAccess, String[] extrasKeys, String[] extrasValues, int extrasCount) {
        if (rewardedAd == null) return;
        VungleCSBData.Builder builder = new VungleCSBData.Builder(bidFloor)
            .auctionId(auctionId)
            .creativeId(creativeId)
            .adUnitId(adUnitId)
            .isVXWinner(isVxWinner)
            .isPriorityAccess(isPriorityAccess);
        if (extrasKeys != null && extrasValues != null && extrasCount > 0) {
            Map<String, String> extras = new HashMap<>();
            for (int i = 0; i < extrasCount; i++) {
                extras.put(extrasKeys[i], extrasValues[i]);
            }
            builder.putExtras(extras);
        }
        rewardedAd.load(builder.build());
    }

    public double getWinningPrice() {
        if (rewardedAd == null) return 0;
        return rewardedAd.getWinningPrice();
    }

    public void sendWinURL() {
        if (rewardedAd == null) return;
        rewardedAd.sendWinURL();
    }

    public void sendLossURL() {
        if (rewardedAd == null) return;
        rewardedAd.sendLossURL();
    }

    public void playAd() {
        rewardedAd.play(getContext());
    }

    @Override
    public void onAdLoaded(BaseAd baseAd) {
        callbackReceiver.RewardedLoadedCallback();
    }

    @Override
    public void onAdFailedToLoad(BaseAd baseAd, VungleError vungleError) {
        callbackReceiver.RewardedFailedToLoadCallback(vungleError.getErrorMessage());
    }

    @Override
    public void onAdStart(BaseAd baseAd) {
        callbackReceiver.RewardedDidPresentCallback();
    }

    @Override
    public void onAdEnd(BaseAd baseAd) {
        callbackReceiver.RewardedDidCloseCallback();
        rewardedAd.setAdListener(null);
        rewardedAd = null;
        callbackReceiver = null;
    }

    @Override
    public void onAdFailedToPlay(BaseAd baseAd, VungleError vungleError) {
        callbackReceiver.RewardedFailedToPresentCallback(vungleError.getErrorMessage());
    }

    @Override
    public void onAdImpression(BaseAd baseAd) {
        callbackReceiver.RewardedDidTrackImpressionCallback();
    }

    @Override
    public void onAdClicked(BaseAd baseAd) {
        callbackReceiver.RewardedDidClickCallback();
    }

    @Override
    public void onAdLeftApplication(BaseAd baseAd) {
        callbackReceiver.RewardedWillLeaveApplicationCallback();
    }

    @Override
    public void onAdRewarded(BaseAd baseAd) {
        callbackReceiver.RewardedDidRewardUserCallback();
    }
}
