using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace VungleAds
{
    #if UNITY_IOS

    public partial class VungleSdkIos : IVungleSdk
    {
        internal delegate void VungleInitializationCompleteCallback(IntPtr sdkPtr);
        internal delegate void VungleInitializationFailureCallback(IntPtr sdkPtr, string message);

        private IntPtr sdkPtr;
        private Action onInitializeSuccessEvent;
        private Action<string> onInitializeFailedEvent;

        public VungleSdkIos()
        {
            this.sdkPtr = (IntPtr)GCHandle.Alloc(this);
        }

        ~VungleSdkIos()
        {
            ((GCHandle)this.sdkPtr).Free();
        }

        public void Init(string appId)
        {
            this.onInitializeSuccessEvent = VungleSdk.onInitializeSuccessEvent;
            this.onInitializeFailedEvent = VungleSdk.onInitializeFailedEvent;
            InitializeVungleSdk(
                sdkPtr,
                appId,
                VungleSdk.PluginVersion,
                InitializationCompleteCallback,
                InitializationFailureCallback);
        }

        [MonoPInvokeCallback(typeof(VungleInitializationCompleteCallback))]
        private static void InitializationCompleteCallback(IntPtr sdkPtr)
        {
            GCHandle handle = (GCHandle)sdkPtr;
            VungleSdkIos pluginSdk = handle.Target as VungleSdkIos;
            if (pluginSdk == null) return;
            pluginSdk.onInitializeSuccessEvent?.Invoke();
            pluginSdk.onInitializeSuccessEvent = null;
            pluginSdk.onInitializeFailedEvent = null;
        }

        [MonoPInvokeCallback(typeof(VungleInitializationFailureCallback))]
        private static void InitializationFailureCallback(IntPtr sdkPtr, string message)
        {
            GCHandle handle = (GCHandle)sdkPtr;
            VungleSdkIos pluginSdk = handle.Target as VungleSdkIos;
            if (pluginSdk == null) return;
            pluginSdk.onInitializeFailedEvent?.Invoke(message);
            pluginSdk.onInitializeSuccessEvent = null;
            pluginSdk.onInitializeFailedEvent = null;
        }

        #region DllImports

        [DllImport("__Internal")]
        private static extern void InitializeVungleSdk(
            IntPtr sdkPtr,
            string appId,
            string pluginVersion,
            VungleInitializationCompleteCallback successCallback,
            VungleInitializationFailureCallback failCallback);

        #endregion
    }

    #endif
}
