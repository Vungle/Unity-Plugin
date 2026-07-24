package com.vungle.androidplugin;

public interface IVungleInitCallbackReceiver {
    void VungleInitializationSuccessCallback();
    void VungleInitializationFailedCallback(String error);
}
