using System;

namespace VungleAds
{
    public interface IVungleSdk
    {
    	void Init(string appId);
    }

    public static partial class VungleSdk
    {
    	public const string PluginVersion = "7.0.0.0";

    	static IVungleSdk sdk;

    	public static Action onInitializeSuccessEvent;
    	public static Action<string> onInitializeFailedEvent;

    	static VungleSdk()
    	{
    #if UNITY_EDITOR
    		sdk = new VungleUnityEditor();
    #elif UNITY_IOS
    		sdk = new VungleSdkIos();
    #elif UNITY_ANDROID
    		sdk = new VungleSdkAndroid();
    #else
    		sdk = new VungleUnityEditor();
    #endif
    	}

    	public static void Init(string appId)
    	{
    		sdk.Init(appId);
    	}
    }
}
