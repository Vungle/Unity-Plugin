using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;


#if UNITY_IPHONE
public enum VungleAdOrientation
{
	Portrait = 1,
    LandscapeLeft = 2,
    LandscapeRight = 3,
    PortraitUpsideDown = 4,
    Landscape = 5,
    All = 6,
    AllButUpsideDown = 7
}

public class VungleBinding
{
	static VungleBinding()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			VungleManager.noop();
	}


	[DllImport("__Internal")]
	private static extern void _vungleStartWithAppId(string appId, string pluginVersion, bool initHeaderBiddingDelegate);
	// Starts up the SDK with the given appId
    public static void startWithAppId(string appId, string pluginVersion)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            _vungleStartWithAppId(appId, pluginVersion, false);
    }
    public static void startWithAppId(string appId, string pluginVersion, bool initHeaderBiddingDelegate)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            _vungleStartWithAppId(appId, pluginVersion, initHeaderBiddingDelegate);
    }
		
	[DllImport("__Internal")]
	private static extern void _vungleSetSoundEnabled( bool enabled );

	// Enables/disables sound
	public static void setSoundEnabled( bool enabled )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetSoundEnabled( enabled );
	}
		
	[DllImport("__Internal")]
	private static extern void _vungleEnableLogging( bool shouldEnable );

	// Enables/disables verbose logging
	public static void enableLogging( bool shouldEnable )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleEnableLogging( shouldEnable );
	}

	[DllImport("__Internal")]
	private static extern bool _vungleIsAdAvailable(string placementID);

	// Checks to see if a video ad is available
	public static bool isAdAvailable(string placementID)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _vungleIsAdAvailable(placementID);
		return false;
	}
	
	[DllImport("__Internal")]
	private static extern void _vungleLoadAd(string placementID);
	
	// Loads an ad
	public static void loadAd(string placementID) {
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleLoadAd(placementID);
	}
	
	[DllImport("__Internal")]
	private static extern bool _vungleCloseAd(string placementID);
	
	// Loads an ad
	public static bool closeAd(string placementID) {
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _vungleCloseAd(placementID);
		return false;
	}
	
	[DllImport("__Internal")]
	private static extern void _vunglePlayAd(string options, string placementID);
	
	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd(string placementID) {
		Dictionary<string,object> options = new Dictionary<string,object> ();
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vunglePlayAd(MiniJSONV.Json.Serialize(options), placementID);
	}

	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd(Dictionary<string,object> options, string placementID) {
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vunglePlayAd(MiniJSONV.Json.Serialize(options), placementID);
	}

    [DllImport("__Internal")]
    private static extern void _updateConsentStatus(int consent, string version);
    public static void updateConsentStatus(Vungle.Consent consent, string version="1.0")
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer ) {
            if (Vungle.Consent.Undefined == consent) return;
            _updateConsentStatus((int)consent, version);
        }
    }

    [DllImport("__Internal")]
    private static extern int _getConsentStatus();
    public static Vungle.Consent getConsentStatus()
    {
        if( Application.platform == RuntimePlatform.IPhonePlayer )
            return (Vungle.Consent)_getConsentStatus();
        return Vungle.Consent.Undefined;
    }

    [DllImport("__Internal")]
	private static extern void _vungleClearSleep();
	
	public static void clearSleep()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleClearSleep();
	}
	
	[DllImport("__Internal")]
	private static extern void _vungleSetEndPoint(string endPoint);
	
	public static void setEndPoint(string endPoint)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetEndPoint(endPoint);
	}

	[DllImport("__Internal")]
	private static extern string _vungleGetEndPoint();
	
	public static string getEndPoint()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _vungleGetEndPoint();
		return "";
	}
	
	[DllImport("__Internal")]
	private static extern void _vungleSetPublishIDFV( bool shouldEnable );
	
	public static void SetPublishIDFV(bool shouldEnable)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetPublishIDFV(shouldEnable);
	}
	
	[DllImport("__Internal")]
	private static extern void _vungleSetMinimumSpaceForInit(int minimumSize);
	
	public static void SetMinimumSpaceForInit(int minimumSize)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetMinimumSpaceForInit(minimumSize);
	}

	[DllImport("__Internal")]
	private static extern void _vungleSetMinimumSpaceForAd(int minimumSize);
	
	public static void SetMinimumSpaceForAd(int minimumSize)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetMinimumSpaceForAd(minimumSize);
	}
	
}
#endif
