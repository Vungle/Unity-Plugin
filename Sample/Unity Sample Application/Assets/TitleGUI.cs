using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// To ensure proper behavior of the Vungle SDK, please target an iOS, Android, or Windows platform in the Unity Editor.
public class TitleGUI : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO

	// These AppIDs point to Vungle test applications on the dashboard.
	// Replace these with your own AppIDs to test your app's dashboard settings.
	string iOSAppID = "5e13cc9d61880b27a65bf735";
	string androidAppID = "5ae0db55e2d43668c97bd65e";
	string windowsAppID = "59792a4f057243276200298a";

	// These PlacementIDs point to Vungle test applications on the dashboard.
	// Replace these with your own PlacementIDs to test your placements' dashboard settings.
#if UNITY_IPHONE
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT-2227894", false },
		{ "REWARDED02-4065911", false },
		{ "REWARDED_PLAYABLE01-4686142", false }
	};

#elif UNITY_ANDROID
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT-6595425", false },
		{ "DYNAMIC_TEMPLATE_INTERSTITIAL-6969365", false },
		{ "DYNAMIC_TEMPLATE_REWARDED-5271535", false }
	};

#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
    Dictionary<string, bool> placements = new Dictionary<string, bool>
    {
        { "DEFAULT18154", false },
        { "PLACEME92007", false },
        { "REWARDP93292", false }
    };

#endif

	public Button initSDKButton;
	public Button playPlacement1Button;
	public Button loadPlacement2Button;
	public Button playPlacement2Button;
	public Button loadPlacement3Button;
	public Button playPlacement3Button;
	public Button toBannersCanvas;
	public Text appIDText;
	public Text placementID1Text;
	public Text placementID2Text;
	public Text placementID3Text;

	List<string> placementIdList;

	bool adInited = false;

	void Start()
	{
		SetupButtonsAndText();
	}

	void Update()
	{
		updateButtonState();
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			Vungle.onPause();
		}
		else
		{
			Vungle.onResume();
		}
	}

	// UI initialization
	void SetupButtonsAndText()
	{
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
		placementID1Text.text = "Placement ID: " + placementIdList[0];
		placementID2Text.text = "Placement ID: " + placementIdList[1];
		placementID3Text.text = "Placement ID: " + placementIdList[2];

		onInit();

		playPlacement1Button.onClick.AddListener(onPlayPlacement1);
		loadPlacement2Button.onClick.AddListener(onLoadPlacement2);
		playPlacement2Button.onClick.AddListener(onPlayPlacement2);
		loadPlacement3Button.onClick.AddListener(onLoadPlacement3);
		playPlacement3Button.onClick.AddListener(onPlayPlacement3);
		toBannersCanvas.onClick.AddListener(showBannerCanvas);
	}

	// Vungle SDK initialization
	// Uses an AppID for iOS, Android, or Windows depending on platform
	void onInit()
	{
		DebugLog("Initializing the Vungle SDK");

		string appID;

#if UNITY_IPHONE
		appID = iOSAppID;
#elif UNITY_ANDROID
		appID = androidAppID;
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
		appID = windowsAppID;
#endif

		// attach event handlers prior to init.
		initializeEventHandlers();
		// Initialize Vungle SDK
		Vungle.init(appID);
	}

	void onPlayPlacement1()
	{
		Vungle.playAd(placementIdList[0]);
	}


	void onLoadPlacement2()
	{
		Vungle.loadAd(placementIdList[1]);
	}


	void onPlayPlacement2()
	{
		// option to change orientation
		Dictionary<string, object> options = new Dictionary<string, object>();
#if UNITY_IPHONE
		options["orientation"] = 6;
#else
		options["orientation"] = true;
#endif

		Vungle.playAd(options, placementIdList[1]);
	}

	void onLoadPlacement3()
	{
		Vungle.loadAd(placementIdList[2]);
	}


	void onPlayPlacement3()
	{
		// option to customize alert window and send user_id
		Dictionary<string, object> options = new Dictionary<string, object>();
 		options ["userTag"] = "test_user_id";
 		options ["alertTitle"] = "Careful!";
 		options ["alertText"] = "If the video isn't completed you won't get your reward! Are you sure you want to close early?";
 		options ["closeText"] = "Close";
 		options ["continueText"] = "Keep Watching";

		Vungle.playAd(options, placementIdList[2]);
	}

	void showBannerCanvas()
	{
		ViewManager.Instance.LoadView(ViewManager.ViewId.BannerAd);
	}

	void updateButtonState()
	{
		playPlacement1Button.interactable = placements[placementIdList[0]];
		loadPlacement2Button.interactable = adInited & !placements[placementIdList[1]];
		playPlacement2Button.interactable = placements[placementIdList[1]];
		loadPlacement3Button.interactable = adInited & !placements[placementIdList[2]];
		playPlacement3Button.interactable = placements[placementIdList[2]];
	}

	// Setup EventHandlers for all available Vungle events
	void initializeEventHandlers()
	{

		// Event triggered during when an ad is about to be played
		Vungle.onAdStartedEvent += (placementID) =>
		{
			DebugLog("onAdStartedEvent " + placementID + " is starting!  Pause your game  animation or sound here.");

#if UNITY_ANDROID
			placements[placementID] = false;
#endif
		};

		// Event is triggered when a Vungle ad finished and provides the entire information about this event
		// These can be used to determine how much of the video the user viewed, if they skipped the ad early, etc.
		Vungle.onAdFinishedEvent += (placementID, args) =>
		{
			DebugLog("onAdFinishedEvent - placementID " + placementID + ", was call to action clicked:" + args.WasCallToActionClicked + ", is completed view:"
				+ args.IsCompletedView);
			updateButtonState();
			Vungle.loadAd(placementIdList[1]);
		};

		// Event is triggered when the ad's playable state has been changed
		// It can be used to enable certain functionality only accessible when ad plays are available
		Vungle.adPlayableEvent += (placementID, adPlayable) =>
		{
			DebugLog("adPlayableEvent - Ad's playable state has been changed! placementID " + placementID + ". Now: " + adPlayable);
			placements[placementID] = adPlayable;
		};

		//Fired initialize event from sdk
		Vungle.onInitializeEvent += () =>
		{
			adInited = true;
			toBannersCanvas.interactable = true;
			DebugLog("onInitializeEvent - SDK initialized");

			// GDPR and CCPA Consent API
			// string version = "1.0.0";
			// Vungle.updateConsentStatus(Vungle.Consent.Denied, version);
			// Vungle.updateCCPAStatus(Vungle.Consent.Denied);

		};

		// Event is trigger when user has click on the ad
		Vungle.onAdClickEvent += (placementID) => 
		{
			DebugLog("onClick - Log: " + placementID);
		};

		// Event is trigger for rewarded video when the user should be rewarded for completing video ad
		Vungle.onAdRewardedEvent += (placementID) => 
		{
			DebugLog("onAdRewardedEvent - Log: " + placementID);
		};

		// Event is trigger when the entire ad experience has ended
		Vungle.onAdEndEvent += (placementID) => 
		{
			DebugLog("onAdEnd - Log: " + placementID); 
		};

		// Diagnostic events for debugging
		Vungle.onLogEvent += (log) =>
		{
			DebugLog("onLogEvent - Log: " + log);
		};
	}

	/* Common method for ensuring logging messages have the same format */
	void DebugLog(string message)
	{
		Debug.Log("VungleUnity" + System.DateTime.Now + ": " + message);
	}

#endif

}
