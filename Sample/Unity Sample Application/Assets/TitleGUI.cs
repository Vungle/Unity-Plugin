using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_IPHONE || UNITY_ANDROID
public class TitleGUI : MonoBehaviour {
	
	string iOSAppID = "5912326f0e96c1a540000014";
	string androidAppID = "591236625b2480ac40000028";
	string windowsAppID = "";

#if UNITY_IPHONE
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT63997", false },
		{ "PLMT02I58266", false },
		{ "PLMT03R65406", false }
	};
#else
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT18080", false },
		{ "PLMT02I58745", false },
		{ "PLMT03R02739", false }
	};
#endif

	List<string> placementIdList;
	
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
	
	bool adInited = false;


	void Start () {
		SetupButtonsAndText();
	}
	
	// Update is called once per frame
	void Update () {
		updateButtonState ();
	}

	// Called when the player pauses
	void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus) {
			Vungle.onPause ();
		} else {
			Vungle.onResume ();
		}
	}

	void SetupButtonsAndText () {
		placementIdList = new List<string>(placements.Keys);

		string appID;
#if UNITY_IPHONE
		appID = iOSAppID;
#else
		appID = androidAppID;
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
	
	void onInitButton() {
		DebugLog("Initializing the Vungle SDK");

		initSDKButton.interactable = false;

		string[] array = new string[placements.Keys.Count];
		placements.Keys.CopyTo(array, 0);
		Vungle.init (androidAppID, iOSAppID, windowsAppID, array);
		initializeEventHandlers ();
	}
	
	void onPlayPlacement1 () {
		Vungle.playAd(placementIdList[0]);
	}
	
	void onLoadPlacement2 () {
		Vungle.loadAd(placementIdList[1]);
	}
	
	void onPlayPlacement2 () {
		Vungle.playAd(placementIdList[1]);
	}
	
	void onLoadPlacement3 () {
		Vungle.loadAd(placementIdList[2]);
	}
	
	void onPlayPlacement3 () {
		Vungle.playAd(placementIdList[2]);
	}
	
	void updateButtonState() {
		playPlacement1Button.interactable = placements[placementIdList[0]];
		loadPlacement2Button.interactable = adInited & !placements[placementIdList[1]];
		playPlacement2Button.interactable = placements[placementIdList[1]];
		loadPlacement3Button.interactable = adInited & !placements[placementIdList[2]];
		playPlacement3Button.interactable = placements[placementIdList[2]];
	}

	/* Setup EventHandlers for all available Vungle events */
	void initializeEventHandlers() {

		//Event triggered during when an ad is about to be played
		Vungle.onAdStartedEvent += (placementID) => {
			DebugLog ("Ad " + placementID + " is starting!  Pause your game  animation or sound here.");
		};

		//Event is triggered when a Vungle ad finished and provides the entire information about this event
		//These can be used to determine how much of the video the user viewed, if they skipped the ad early, etc.
		Vungle.onAdFinishedEvent += (placementID, args) => {
			DebugLog ("Ad finished - placementID " + placementID + " watched time:" + args.TimeWatched + ", was call to action clicked:" + args.WasCallToActionClicked +  ", is completed view:" 
			          + args.IsCompletedView);
		};

		//Event is triggered when the ad's playable state has been changed
		//It can be used to enable certain functionality only accessible when ad plays are available
		Vungle.adPlayableEvent += (placementID, adPlayable) => {
			DebugLog ("Ad's playable state has been changed! placementID " + placementID + ". Now: " + adPlayable);
			placements[placementID] = adPlayable;
		};

		//Fired log event from sdk
		Vungle.onLogEvent += (log) => {
			DebugLog ("Log: " + log);
		};

		//Fired initialize event from sdk
		Vungle.onInitializeEvent += () => {
			adInited = true;
			DebugLog ("SDK initialized");
		};
		
	}

	/* Common method for ensuring logging messages have the same format */
	void DebugLog(string message) {
		Debug.Log("VungleUnitySample " + System.DateTime.Today +": " + message);
	}
}
#endif
