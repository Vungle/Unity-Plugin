package com.vungle.androidplugin;

import android.app.Activity;

import com.vungle.ads.VungleAds;
import com.vungle.ads.InitializationListener;
import com.vungle.ads.VungleError;

public class VunglePluginSDK {
    private static VunglePluginSDK instance;

    public static VunglePluginSDK instance() {
        if (instance == null) {
            instance = new VunglePluginSDK();
        }
        return instance;
    }

    public static Activity getActivity() {
        return com.unity3d.player.UnityPlayer.currentActivity;
    }

    public void initSDK(String appId, IVungleInitCallbackReceiver receiver) {
        android.util.Log.d("VunglePlugin", "initSDK appId=" + appId);
        VungleAds.init(getActivity().getApplicationContext(), appId, new InitializationListener() {
            @Override
            public void onSuccess() {
                android.util.Log.d("VunglePlugin", "init onSuccess");
                if (receiver != null) {
                    receiver.VungleInitializationSuccessCallback();
                }
            }
            @Override
            public void onError(VungleError vungleError) {
                android.util.Log.d("VunglePlugin", "init onError: " + vungleError.getErrorMessage());
                if (receiver != null) {
                    receiver.VungleInitializationFailedCallback(vungleError.getErrorMessage());
                }
            }
        });
    }
}
