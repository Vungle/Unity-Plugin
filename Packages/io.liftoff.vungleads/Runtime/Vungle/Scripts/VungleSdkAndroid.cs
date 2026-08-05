using System;
using System.Collections.Generic;
using UnityEngine;

namespace VungleAds
{
    public class VungleSdkAndroid : AndroidJavaProxy, IVungleSdk
    {
        private AndroidJavaObject sdkObject;
        public static Action onInitializeSuccessEvent;
        public static Action<string> onInitializeFailedEvent;

        public VungleSdkAndroid() : base("com.vungle.androidplugin.IVungleInitCallbackReceiver") {
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginSDK");
            sdkObject = vungleClass.CallStatic<AndroidJavaObject>("instance");
        }

        public void Init(string appId)
        {
            onInitializeSuccessEvent = VungleSdk.onInitializeSuccessEvent;
            onInitializeFailedEvent = VungleSdk.onInitializeFailedEvent;
            sdkObject.Call("initSDK", appId, this);
        }

        public void VungleInitializationSuccessCallback()
        {
            var callback = onInitializeSuccessEvent;
            onInitializeSuccessEvent = null;
            onInitializeFailedEvent = null;
            VungleThreadDispatcher.Enqueue(() => callback?.Invoke());
        }

        public void VungleInitializationFailedCallback(string error)
        {
            var callback = onInitializeFailedEvent;
            onInitializeSuccessEvent = null;
            onInitializeFailedEvent = null;
            VungleThreadDispatcher.Enqueue(() => callback?.Invoke(error));
        }
    }
}
