package com.vungle.androidplugin;

import android.app.Activity;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import java.util.ArrayList;
import java.util.List;

import com.vungle.ads.BaseAd;
import com.vungle.ads.NativeAd;
import com.vungle.ads.NativeAdListener;
import com.vungle.ads.VungleError;
import com.vungle.ads.internal.ui.view.MediaView;

public class VunglePluginNativeAd implements NativeAdListener {
    private NativeAd nativeAd;
    private IVungleNativeCallbackReceiver callbackReceiver;
    private FrameLayout containerView;
    private MediaView mediaView;
    private final List<View> clickableOverlays = new ArrayList<>();
    private boolean registered = false;
    private boolean adLoaded = false;

    VunglePluginNativeAd(String placementId, IVungleNativeCallbackReceiver receiver) {
        nativeAd = new NativeAd(getActivity(), placementId);
        nativeAd.setAdListener(this);
        callbackReceiver = receiver;
    }

    public static VunglePluginNativeAd createInstance(String placementId, IVungleNativeCallbackReceiver receiver) {
        return new VunglePluginNativeAd(placementId, receiver);
    }

    private static Activity getActivity() {
        return com.unity3d.player.UnityPlayer.currentActivity;
    }

    public boolean canPlayAd() {
        return adLoaded && nativeAd != null;
    }

    public void loadAd() {
        NativeAd ad = nativeAd;
        if (ad != null) {
            ad.load();
        }
    }

    public String getAdTitle() {
        NativeAd ad = nativeAd;
        return ad != null && ad.getAdTitle() != null ? ad.getAdTitle() : "";
    }

    public String getAdBodyText() {
        NativeAd ad = nativeAd;
        return ad != null && ad.getAdBodyText() != null ? ad.getAdBodyText() : "";
    }

    public String getAdCallToActionText() {
        NativeAd ad = nativeAd;
        return ad != null && ad.getAdCallToActionText() != null ? ad.getAdCallToActionText() : "";
    }

    public double getAdStarRating() {
        NativeAd ad = nativeAd;
        Double rating = ad != null ? ad.getAdStarRating() : null;
        return rating != null ? rating : 0.0;
    }

    public String getAppIcon() {
        NativeAd ad = nativeAd;
        return ad != null && ad.getAppIcon() != null ? ad.getAppIcon() : "";
    }

    public void attach(int x, int y, int width, int height) {
        attach(x, y, width, height, x, y, width, height, new int[0]);
    }

    // clickableRects: flattened screen-coordinate rects (x, y, w, h per entry).
    // A transparent overlay view is placed in the container for each rect and
    // registered as clickable with the SDK; the media view is always clickable.
    public void attach(int x, int y, int width, int height,
                       int mediaX, int mediaY, int mediaWidth, int mediaHeight,
                       int[] clickableRects) {
        Activity activity = getActivity();
        int clickableCount = clickableRects == null ? 0 : clickableRects.length / 4;
        activity.runOnUiThread(() -> {
            NativeAd ad = nativeAd;
            if (ad == null) {
                return;
            }
            FrameLayout rootView = activity.findViewById(android.R.id.content);
            boolean rebuild = containerView == null || !registered
                || clickableCount != clickableOverlays.size();

            // Changing the registered view set requires a quiet unregister and
            // fresh views: re-registering the same rootView while playing is a
            // silent no-op in the SDK, and unregisterView destroys the media
            // content. The listener is detached around unregisterView because
            // the SDK fires onAdEnd synchronously from it — this is a
            // re-registration, not a real close, so it must not reach Unity or
            // re-enter our handler. The ad lands in READY state and can be
            // re-registered immediately.
            if (rebuild && containerView != null && registered) {
                ad.setAdListener(null);
                ad.unregisterView();
                ad.setAdListener(VunglePluginNativeAd.this);
                registered = false;
                ViewGroup oldParent = (ViewGroup) containerView.getParent();
                if (oldParent != null) {
                    oldParent.removeView(containerView);
                }
                clickableOverlays.clear();
                containerView = null;
                mediaView = null;
            }

            boolean firstAttach = (containerView == null);

            if (firstAttach) {
                containerView = new FrameLayout(activity);
                containerView.setClipChildren(true);
                mediaView = new MediaView(activity);
                containerView.addView(mediaView, new FrameLayout.LayoutParams(
                    FrameLayout.LayoutParams.MATCH_PARENT,
                    FrameLayout.LayoutParams.MATCH_PARENT));
            }

            // Size MediaView to respect content aspect ratio, within the media
            // rect (given in screen coordinates, converted to container-relative)
            float ratio = ad.getMediaAspectRatio();
            int mw = mediaWidth, mh = mediaHeight;
            if (ratio > 0) {
                if ((float) mediaWidth / mediaHeight > ratio) {
                    mh = mediaHeight;
                    mw = (int) (mediaHeight * ratio);
                } else {
                    mw = mediaWidth;
                    mh = (int) (mediaWidth / ratio);
                }
            }
            FrameLayout.LayoutParams mediaParams = new FrameLayout.LayoutParams(mw, mh);
            mediaParams.leftMargin = (mediaX - x) + (mediaWidth - mw) / 2;
            mediaParams.topMargin = (mediaY - y) + (mediaHeight - mh) / 2;
            mediaParams.gravity = Gravity.TOP | Gravity.LEFT;
            mediaView.setLayoutParams(mediaParams);

            if (rebuild) {
                for (View overlay : clickableOverlays) {
                    containerView.removeView(overlay);
                }
                clickableOverlays.clear();
                for (int i = 0; i < clickableCount; i++) {
                    View overlay = new View(activity);
                    containerView.addView(overlay);
                    clickableOverlays.add(overlay);
                }
            }
            for (int i = 0; i < clickableCount; i++) {
                FrameLayout.LayoutParams overlayParams = new FrameLayout.LayoutParams(
                    clickableRects[i * 4 + 2], clickableRects[i * 4 + 3]);
                overlayParams.leftMargin = clickableRects[i * 4] - x;
                overlayParams.topMargin = clickableRects[i * 4 + 1] - y;
                overlayParams.gravity = Gravity.TOP | Gravity.LEFT;
                clickableOverlays.get(i).setLayoutParams(overlayParams);
            }

            FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(width, height);
            params.leftMargin = x;
            params.topMargin = y;
            params.gravity = Gravity.TOP | Gravity.LEFT;

            if (containerView.getParent() == rootView) {
                containerView.setLayoutParams(params);
            } else {
                ViewGroup parent = (ViewGroup) containerView.getParent();
                if (parent != null) {
                    parent.removeView(containerView);
                }
                rootView.addView(containerView, params);
            }

            // Register after the view is in the hierarchy so the SDK can render
            // (any prior registration was quietly unregistered above)
            if (rebuild) {
                ad.registerViewForInteraction(containerView, mediaView, null,
                    clickableOverlays.isEmpty() ? null : new ArrayList<>(clickableOverlays));
                registered = true;
            }
        });
    }

    // Hides the ad but keeps the views and SDK registration so a later attach
    // can show it again cheaply. Use destroy() to release the ad entirely.
    public void detach() {
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            if (containerView != null) {
                ViewGroup parent = (ViewGroup) containerView.getParent();
                if (parent != null) {
                    parent.removeView(containerView);
                }
            }
        });
    }

    // Full teardown: unregisters from the SDK (releasing the rendered media
    // content, ad options view, and click listeners), removes the views, and
    // detaches the listener so no further callbacks reach Unity.
    public void destroy() {
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            // Detach the listener FIRST: unregisterView fires onAdEnd
            // synchronously, which must not re-enter our teardown or reach
            // Unity for an ad the publisher already discarded
            NativeAd ad = nativeAd;
            if (ad != null) {
                ad.setAdListener(null);
                if (registered) {
                    ad.unregisterView();
                    registered = false;
                }
            }
            if (containerView != null) {
                ViewGroup parent = (ViewGroup) containerView.getParent();
                if (parent != null) {
                    parent.removeView(containerView);
                }
            }
            clickableOverlays.clear();
            containerView = null;
            mediaView = null;
            nativeAd = null;
            callbackReceiver = null;
        });
    }

    @Override
    public void onAdLoaded(BaseAd baseAd) {
        adLoaded = true;
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver == null) {
            return;
        }
        receiver.NativeLoadedCallback();
        receiver.NativeAdDataCallback(
            getAdTitle(),
            getAdBodyText(),
            getAdCallToActionText(),
            getAdStarRating(),
            getAppIcon()
        );
    }

    @Override
    public void onAdFailedToLoad(BaseAd baseAd, VungleError vungleError) {
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeFailedToLoadCallback(vungleError.getErrorMessage());
        }
    }

    @Override
    public void onAdStart(BaseAd baseAd) {
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeDidPresentCallback();
        }
    }

    @Override
    public void onAdEnd(BaseAd baseAd) {
        // Only hide the views here — full teardown (nulling nativeAd and the
        // callback receiver) is destroy()'s job. onAdEnd can fire reentrantly
        // from inside unregisterView, and nulling shared fields here made
        // destroy()'s continuation NPE.
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeDidCloseCallback();
        }
        adLoaded = false;
        Activity activity = getActivity();
        activity.runOnUiThread(() -> {
            if (containerView != null && containerView.getParent() != null) {
                ((ViewGroup) containerView.getParent()).removeView(containerView);
            }
            clickableOverlays.clear();
            containerView = null;
            mediaView = null;
            registered = false;
        });
    }

    @Override
    public void onAdFailedToPlay(BaseAd baseAd, VungleError vungleError) {
        // Registration failed (not loaded, expired, ...) — clear the flag so
        // the next attach re-registers instead of assuming the SDK is wired
        registered = false;
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeFailedToPresentCallback(vungleError.getErrorMessage());
        }
    }

    @Override
    public void onAdImpression(BaseAd baseAd) {
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeDidTrackImpressionCallback();
        }
    }

    @Override
    public void onAdClicked(BaseAd baseAd) {
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeDidClickCallback();
        }
    }

    @Override
    public void onAdLeftApplication(BaseAd baseAd) {
        IVungleNativeCallbackReceiver receiver = callbackReceiver;
        if (receiver != null) {
            receiver.NativeWillLeaveApplicationCallback();
        }
    }
}
