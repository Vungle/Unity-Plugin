package com.vungle.androidplugin;

import android.app.Activity;
import android.view.Gravity;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import com.vungle.ads.BaseAd;
import com.vungle.ads.BannerAdListener;
import com.vungle.ads.VungleAdSize;
import com.vungle.ads.VungleBannerView;
import com.vungle.ads.VungleError;

public class VunglePluginBannerView implements BannerAdListener {
    private VungleBannerView bannerView;
    private IVungleBannerViewCallbackReceiver callbackReceiver;

    VunglePluginBannerView(String placementId, int adSizeType, int width, int height, IVungleBannerViewCallbackReceiver receiver) {
        VungleAdSize adSize = getAdSize(adSizeType, width, height);
        bannerView = new VungleBannerView(getActivity(), placementId, adSize);
        bannerView.setAdListener(this);
        callbackReceiver = receiver;
    }

    public static VunglePluginBannerView createInstance(String placementId, int adSizeType, int width, int height, IVungleBannerViewCallbackReceiver receiver) {
        return new VunglePluginBannerView(placementId, adSizeType, width, height, receiver);
    }

    private static Activity getActivity() {
        return com.unity3d.player.UnityPlayer.currentActivity;
    }

    private VungleAdSize getAdSize(int adSizeType, int width, int height) {
        switch (adSizeType) {
            case 1: return VungleAdSize.BANNER_SHORT;
            case 2: return VungleAdSize.BANNER_LEADERBOARD;
            case 3: return VungleAdSize.MREC;
            case 4: return VungleAdSize.getAdSizeWithWidth(getActivity(), width);
            case 5: return VungleAdSize.getAdSizeWithWidthAndHeight(width, height);
            default: return VungleAdSize.BANNER;
        }
    }

    public void loadAd() {
        bannerView.load();
    }

    public void attach(int x, int y, int width, int height) {
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            FrameLayout rootView = activity.findViewById(android.R.id.content);
            int w = width > 0 ? width : FrameLayout.LayoutParams.WRAP_CONTENT;
            int h = height > 0 ? height : FrameLayout.LayoutParams.WRAP_CONTENT;
            FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(w, h);
            params.leftMargin = x;
            params.topMargin = y;
            params.gravity = Gravity.TOP | Gravity.LEFT;

            if (bannerView.getParent() == rootView) {
                bannerView.setLayoutParams(params);
            } else {
                ViewGroup parent = (ViewGroup) bannerView.getParent();
                if (parent != null) {
                    parent.removeView(bannerView);
                }
                rootView.addView(bannerView, params);
            }
        });
    }

    public void detach() {
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            ViewGroup parent = (ViewGroup) bannerView.getParent();
            if (parent != null) {
                parent.removeView(bannerView);
            }
        });
    }

    public void destroy() {
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            ViewGroup parent = (ViewGroup) bannerView.getParent();
            if (parent != null) {
                parent.removeView(bannerView);
            }
            bannerView.finishAd();
        });
    }

    @Override
    public void onAdLoaded(BaseAd baseAd) {
        // Add off-screen immediately after load to trigger onAttachedToWindow and advance
        // the SDK state machine before the caller positions the banner.
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            if (bannerView.getParent() == null) {
                FrameLayout rootView = activity.findViewById(android.R.id.content);
                FrameLayout.LayoutParams init = new FrameLayout.LayoutParams(1, 1);
                init.topMargin = -10000;
                rootView.addView(bannerView, init);
            }
        });
        callbackReceiver.BannerViewLoadedCallback();
    }

    @Override
    public void onAdFailedToLoad(BaseAd baseAd, VungleError vungleError) {
        callbackReceiver.BannerViewFailedToLoadCallback(vungleError.getErrorMessage());
    }

    @Override
    public void onAdStart(BaseAd baseAd) {
        callbackReceiver.BannerViewDidPresentCallback();
    }

    @Override
    public void onAdEnd(BaseAd baseAd) {
        callbackReceiver.BannerViewDidCloseCallback();
    }

    @Override
    public void onAdFailedToPlay(BaseAd baseAd, VungleError vungleError) {
        callbackReceiver.BannerViewFailedToPresentCallback(vungleError.getErrorMessage());
    }

    @Override
    public void onAdImpression(BaseAd baseAd) {
        callbackReceiver.BannerViewDidTrackImpressionCallback();
    }

    @Override
    public void onAdClicked(BaseAd baseAd) {
        callbackReceiver.BannerViewDidClickCallback();
    }

    @Override
    public void onAdLeftApplication(BaseAd baseAd) {
        callbackReceiver.BannerViewWillLeaveApplicationCallback();
    }
}
