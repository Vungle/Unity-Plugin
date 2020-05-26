using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// To ensure proper behavior of the Vungle SDK, please target an iOS, Android, or Windows platform in the Unity Editor.
public class TitleGUI : MonoBehaviour {
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO

	// These AppIDs point to Vungle test applications on the dashboard.
	// Replace these with your own AppIDs to test your app's dashboard settings.
    string iOSAppID = "5af083df47c10a604be1ceb3";
    string androidAppID = "5ae0db55e2d43668c97bd65e";
    string windowsAppID = "59792a4f057243276200298a";

	// These PlacementIDs point to Vungle test applications on the dashboard.
	// Replace these with your own PlacementIDs to test your placements' dashboard settings.
#endif

#if UNITY_IPHONE
    Dictionary<string, bool> placements = new Dictionary<string, bool>
    {
        { "DEFAULT-0547777", false },
        { "DYNAMIC_TEMPLATE_INTERSTITIAL-4765638", false },
        { "LEGACY_REWARDED-1906578", false }
    };
#elif UNITY_ANDROID
    Dictionary<string, bool> placements = new Dictionary<string, bool>
    {
        { "DEFAULT-6595425", false },
        { "LEGACY_INTERSTITIAL-4364832", false },
        { "LEGACY_REWARDED-2115035", false }
    };
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT18154", false },
		{ "PLACEME92007", false },
		{ "REWARDP93292", false }
	};
#endif

#if UNITY_IPHONE
    string banner = "BANNER-8667763";
#elif UNITY_ANDROID
    string banner = "BANNER-5454585";
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
    // Banner and MREC are not yet surported for Windows as of v6.7.0.0-early1
    string banner = "";
#endif

	public Button initSDKButton;
	public Button playPlacement1Button;
	public Button loadPlacement2Button;
	public Button playPlacement2Button;
	public Button loadPlacement3Button;
	public Button playPlacement3Button;
	public Text appIDText;
	public Text placementID1Text;
	public Text placementID2Text;
	public Text placementID3Text;

	List<string> placementIdList;

	bool adInited = false;


	void Start () {
		SetupButtonsAndText();
	}


	void Update () {
		updateButtonState ();
	}


	void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus) {
			Vungle.onPause ();
		} else {
			Vungle.onResume ();
		}
	}

	// UI initialization
	void SetupButtonsAndText () {
		placementIdList = new List<string>(placements.Keys);
		string appID;

#if UNITY_IPHONE
		appID = iOSAppID;
#elif UNITY_ANDROID
		appID = androidAppID;
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
		appID = windowsAppID;
#endif

		appIDText.text = "App ID: " + appID;
		placementID1Text.text = "Placement ID: " + placementIdList [0]; 
		placementID2Text.text = "Placement ID: " + placementIdList [1]; 
		placementID3Text.text = "Placement ID: " + placementIdList [2]; 

		initSDKButton.onClick.AddListener (onInitButton);
		initSDKButton.interactable = true;
		playPlacement1Button.onClick.AddListener (onPlayPlacement1);
		loadPlacement2Button.onClick.AddListener (onLoadPlacement2);
		playPlacement2Button.onClick.AddListener (onPlayPlacement2);
		loadPlacement3Button.onClick.AddListener (onLoadPlacement3);
		playPlacement3Button.onClick.AddListener (onPlayPlacement3);
	}


	// Vungle SDK initialization
	// Uses an AppID for iOS, Android, or Windows depending on platform
	void onInitButton() {
		DebugLog("Initializing the Vungle SDK");

		initSDKButton.interactable = false;
		string appID;

#if UNITY_IPHONE
		appID = iOSAppID;
#elif UNITY_ANDROID
		appID = androidAppID;
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
		appID = windowsAppID;
#endif
		
		// CCPA
        // Vungle.updateCCPAStatus(Vungle.Consent.Denied);
        // DebugLog ("Current CCPA Status - " + Vungle.getCCPAStatus());

		// GDPR
        // string message = "GDPR_consent_version";
        // Vungle.updateConsentStatus(Vungle.Consent.Denied, message);
        // DebugLog ("Current GDPR Status - " + Vungle.getConsentStatus());

        // Optional Advanced Configuration
        // Vungle.SetMinimumDiskSpaceForInitialization(55000);
        // Vungle.SetMinimumDiskSpaceForAd(50000);
        // Vungle.EnableHardwareIdPrivacy(true);
        
		Vungle.init(appID);
		initializeEventHandlers ();
	}


	void onPlayPlacement1 () {
		Vungle.playAd(placementIdList[0]);
	}


	void onLoadPlacement2 () {
		Vungle.loadAd(placementIdList[1]);
	}


	void onPlayPlacement2 () {
		// option to change orientation
		Dictionary<string, object> options = new Dictionary<string, object> ();
#if UNITY_IPHONE
		options ["orientation"] = 5;
#else
		options ["orientation"] = true;
#endif

		Vungle.playAd(options, placementIdList[1]);
	}


	void onLoadPlacement3 () {
		Vungle.loadAd(placementIdList[2]);
	}


	void onPlayPlacement3 () {
		// option to customize alert window and send user_id
		Dictionary<string, object> options = new Dictionary<string, object> ();
		options ["userTag"] = "test_user_id";
		options ["alertTitle"] = "Careful!";
		options ["alertText"] = "If the video isn't completed you won't get your reward! Are you sure you want to close early?";
		options ["closeText"] = "Close";
		options ["continueText"] = "Keep Watching";

		Vungle.playAd(options, placementIdList[2]);
	}


	// Manual implementation of GDPR compliance code
	// To use, attach this function to a button click event
	/*
	void onConsentButton() {
		if(consentState == Vungle.Consent.Accepted) {
			consentState = Vungle.Consent.Denied;
			consentButton.gameObject.GetComponent<Image>().color = Color.red;
			Vungle.updateConsentStatus(consentState);
		}	
		else {
			consentState = Vungle.Consent.Accepted;
			consentButton.gameObject.GetComponent<Image>().color = Color.green;
			Vungle.updateConsentStatus(consentState);
		}

		// Can also set a message sent with all traffic for GDPR versioning
		// string message = "custom_message";
		// Vungle.updateConsentStatus(consentState, message); 
	}	
	*/


	void updateButtonState() {
		playPlacement1Button.interactable = placements[placementIdList[0]];
		loadPlacement2Button.interactable = adInited & !placements[placementIdList[1]];
		playPlacement2Button.interactable = placements[placementIdList[1]];
		loadPlacement3Button.interactable = adInited & !placements[placementIdList[2]];
		playPlacement3Button.interactable = placements[placementIdList[2]];
	}


	// Setup EventHandlers for all available Vungle events
	void initializeEventHandlers() {

		// Event triggered during when an ad is about to be played
		Vungle.onAdStartedEvent += (placementID) => {
			DebugLog ("Ad " + placementID + " is starting!  Pause your game  animation or sound here.");
			#if UNITY_ANDROID
				placements[placementID] = false;
			#endif
		};

		// Event is triggered when a Vungle ad finished and provides the entire information about this event
		// These can be used to determine how much of the video the user viewed, if they skipped the ad early, etc.
		Vungle.onAdFinishedEvent += (placementID, args) => {
			DebugLog ("Ad finished - placementID " + placementID + ", was call to action clicked:" + args.WasCallToActionClicked +  ", is completed view:" 
				+ args.IsCompletedView);
		};

		// Event is triggered when the ad's playable state has been changed
		// It can be used to enable certain functionality only accessible when ad plays are available
		Vungle.adPlayableEvent += (placementID, adPlayable) => {
			DebugLog ("Ad's playable state has been changed! placementID " + placementID + ". Now: " + adPlayable);
			placements[placementID] = adPlayable;

			DebugLog ("isAdvertAvailable " + placementID + " : " + Vungle.isAdvertAvailable(placementID));
			if (placementID == banner) {
                Vungle.showBanner(placementID);
            } 
		};

		//Fired initialize event from sdk
		Vungle.onInitializeEvent += () => {
			adInited = true;
			DebugLog ("SDK initialized");

			Vungle.loadBanner(banner, Vungle.VungleBannerSize.VungleAdSizeBanner, Vungle.VungleBannerPosition.BottomCenter);
		};

		// Other events
		// Vungle.onLogEvent += (log) => {
		// 	 DebugLog ("Log: " + log);
		// };
	}

	/* Common method for ensuring logging messages have the same format */
	void DebugLog(string message) {
		Debug.Log("VungleUnitySample " + System.DateTime.Today +": " + message);
	}
}
