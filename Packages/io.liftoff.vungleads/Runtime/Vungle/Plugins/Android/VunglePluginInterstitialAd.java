package com.vungle.androidplugin;

import android.content.Context;

import java.util.HashMap;
import java.util.Map;

import com.vungle.ads.AdConfig;
import com.vungle.ads.BaseAd;
import com.vungle.ads.InterstitialAd;
import com.vungle.ads.InterstitialAdListener;
import com.vungle.ads.VungleCSBData;
import com.vungle.ads.VungleError;

public class VunglePluginInterstitialAd implements InterstitialAdListener {
    private InterstitialAd interstitialAd;
    private IVungleInterstitialCallbackReceiver callbackReceiver;

    VunglePluginInterstitialAd(String placementId, IVungleInterstitialCallbackReceiver receiver) {
        interstitialAd = new InterstitialAd(getContext(), placementId, new AdConfig());
        interstitialAd.setAdListener(this);
        callbackReceiver = receiver;
    }

    public static VunglePluginInterstitialAd createInstance(String placementId, IVungleInterstitialCallbackReceiver receiver) {
        return new VunglePluginInterstitialAd(placementId, receiver);
    }

    public static Context getContext() {
        return com.unity3d.player.UnityPlayer.currentActivity.getApplicationContext();
    }

    public boolean canPlayAd() {
        return interstitialAd.canPlayAd();
    }

    public void loadAd() {
        interstitialAd.load();
    }

    public void loadAdWithCsb(double bidFloor, String auctionId, String creativeId, String adUnitId, boolean isVxWinner, boolean isPriorityAccess, String[] extrasKeys, String[] extrasValues, int extrasCount) {
        if (interstitialAd == null) return;
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
        interstitialAd.load(builder.build());
    }

    public double getWinningPrice() {
        if (interstitialAd == null) return 0;
        return interstitialAd.getWinningPrice();
    }

    public void sendWinURL() {
        if (interstitialAd == null) return;
        interstitialAd.sendWinURL();
    }

    public void sendLossURL() {
        if (interstitialAd == null) return;
        interstitialAd.sendLossURL();
    }

    public void playAd() {
        interstitialAd.play(getContext());
    }

    @Override
    public void onAdLoaded(BaseAd baseAd) {
        callbackReceiver.InterstitialLoadedCallback();
    }

    @Override
    public void onAdFailedToLoad(BaseAd baseAd, VungleError vungleError) {
        callbackReceiver.InterstitialFailedToLoadCallback(vungleError.getErrorMessage());
    }

    @Override
    public void onAdStart(BaseAd baseAd) {
        callbackReceiver.InterstitialDidPresentCallback();
    }

    @Override
    public void onAdEnd(BaseAd baseAd) {
        callbackReceiver.InterstitialDidCloseCallback();
        interstitialAd.setAdListener(null);
        interstitialAd = null;
        callbackReceiver = null;
    }

    @Override
    public void onAdFailedToPlay(BaseAd baseAd, VungleError vungleError) {
        callbackReceiver.InterstitialFailedToPresentCallback(vungleError.getErrorMessage());
    }

    @Override
    public void onAdImpression(BaseAd baseAd) {
        callbackReceiver.InterstitialDidTrackImpressionCallback();
    }

    @Override
    public void onAdClicked(BaseAd baseAd) {
        callbackReceiver.InterstitialDidClickCallback();
    }

    @Override
    public void onAdLeftApplication(BaseAd baseAd) {
        callbackReceiver.InterstitialWillLeaveApplicationCallback();
    }
}
