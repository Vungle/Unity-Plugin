using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID
public enum VungleGender
{
	None = -1,
	Male = 0,
	Female
}


public enum VungleAdOrientation
{
	AutoRotate,
    MatchVideo
}

public class VungleAndroid
{
	private static AndroidJavaObject _plugin;

	static VungleAndroid() {
		if (Application.platform != RuntimePlatform.Android)
			return;

		VungleManager.noop();

		using(var pluginClass = new AndroidJavaClass("com.vungle.VunglePlugin"))
			_plugin = pluginClass.CallStatic<AndroidJavaObject>("instance");
	}

	// Starts up the SDK with the given appId
	public static void init(string appId, string pluginVersion,
		long? minimumDiskSpaceForInitialization,
		long? minimumDiskSpaceForAd,
		bool? enableHardwareIdPrivacy) {
			
		if (Application.platform != RuntimePlatform.Android)
			return;

		if (minimumDiskSpaceForInitialization.HasValue) {
			_plugin.Call("setMinimumDiskSpaceForInit", minimumDiskSpaceForInitialization.Value);
		}
		if (minimumDiskSpaceForAd.HasValue) {
			_plugin.Call("setMinimumDiskSpaceForAd", minimumDiskSpaceForAd.Value);
		}
		if (enableHardwareIdPrivacy.HasValue) {
			_plugin.Call("setHardwareIdOptOut", enableHardwareIdPrivacy.Value);
		}
		
		_plugin.Call("init", appId, pluginVersion);
	}

	// Call this when your application is sent to the background
	public static void onPause() {
		if (Application.platform != RuntimePlatform.Android)
			return;

		_plugin.Call( "onPause" );
	}

	// Call this when your application resumes
	public static void onResume() {
		if (Application.platform != RuntimePlatform.Android)
			return;

		_plugin.Call( "onResume" );
	}

	// Checks to see if a video is available
	public static bool isVideoAvailable(string placementID) {
		if (Application.platform != RuntimePlatform.Android)
			return false;

		return _plugin.Call<bool>("isVideoAvailable", placementID);
	}

	// Sets if sound should be enabled or not
	public static void setSoundEnabled(bool isEnabled) {
		if (Application.platform != RuntimePlatform.Android)
			return;

		_plugin.Call( "setSoundEnabled", isEnabled );
	}

	// Sets the allowed orientations of any ads that are displayed
	public static void setAdOrientation(VungleAdOrientation orientation) {
		if (Application.platform != RuntimePlatform.Android)
			return;

		_plugin.Call("setAdOrientation", (int)orientation);
	}

	// Checks to see if sound is enabled
	public static bool isSoundEnabled()	{
		if (Application.platform != RuntimePlatform.Android)
			return true;

		return _plugin.Call<bool>("isSoundEnabled");
	}

	// Loads an ad
	public static void loadAd(string placementID) {
		if (Application.platform != RuntimePlatform.Android)
			return;
		
		_plugin.Call("loadAd", placementID);
	}
	
	// Close dlex ad
	public static bool closeAd(string placementID) {
		if (Application.platform != RuntimePlatform.Android)
			return false;
		
		return _plugin.Call<bool>("closeAd", placementID);
	}
	
	public static void playAd(string placementID)
	{
		Dictionary<string,object> options = new Dictionary<string,object> ();
		playAd(options, placementID);
	}
	
	// Plays an ad with the given options.
	public static void playAd(Dictionary<string,object> options, string placementID) {
		if (Application.platform != RuntimePlatform.Android)
			return;
		
		_plugin.Call("playAd", MiniJSONV.Json.Serialize(options), placementID);
	}

    public static void updateConsentStatus(Vungle.Consent consent, string version) {
        if (Application.platform != RuntimePlatform.Android) return;
        if (Vungle.Consent.Undefined == consent) return;

        _plugin.Call("updateConsentStatus", (int)consent, version);
    }

    public static Vungle.Consent getConsentStatus() {
        if (Application.platform != RuntimePlatform.Android) return Vungle.Consent.Undefined;

        return (Vungle.Consent)(_plugin.Call<int>("getConsentStatus"));
    }
	
	public static string getConsentMessageVersion() {
		if (Application.platform != RuntimePlatform.Android) return "";
		
		return _plugin.Call<string>("getConsentMessageVersion");
	}
}
#endif

