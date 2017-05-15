using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IPHONE || UNITY_ANDROID
public class TitleGUI : MonoBehaviour {
#if UNITY_IPHONE
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT63997", false },
		{ "PLMT02I58266", false },
		{ "PLMT03R65406", false }
	};
	string appID = "5912326f0e96c1a540000014";
#else
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "DEFAULT18080", false },
		{ "PLMT02I58745", false },
		{ "PLMT03R02739", false }
	};
	string appID = "591236625b2480ac40000028";
#endif


	//UI Sizing 
	int buttonCount = 3;
	int buttonHeight;
	int headerHeight;
	int spacerHeight = 5;

	//Textures
	Texture2D vungleLogo;
	Texture2D playDefaultAdTexture;
	Texture2D playIncentivizedAdTexture;
	Texture2D playCustomAdTexture;
	Texture2D whiteBackgroundTexture;

	//GUI Styles
	GUIStyle imageButtonStyle;
	GUIStyle titleLabelStyle;
	GUIStyle uiStyle;

	string logTag = "VungleSample-UNITY-";
	// Use this for initialization
	void Start () {

		DebugLog("Initializing the Vungle SDK");

		//Initialize Everything
		initializeTextures ();
		initializeGUIStyles ();
		initializeUISizes ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Called when the player pauses
	void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus)
			Vungle.onPause();
		else
			Vungle.onResume();
	}
	
	void OnGUI () {
//		uiStyle.normal.background = whiteBackgroundTexture;
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height), uiStyle);
		List<string> list = new List<string>(placements.Keys);

		GUI.enabled = true;

		GUILayout.Label ("AppID " + appID, titleLabelStyle);
		if (GUILayout.Button ("Init SDK")) {
			string[] array = new string[placements.Keys.Count];
			placements.Keys.CopyTo(array, 0);
			Vungle.init (appID, appID, appID, array);
			initializeEventHandlers ();
		}

		GUILayout.Label ("Placement 1");
		GUILayout.Label ("PlacementID " + list[0]);
		GUI.enabled = placements[list[0]];
		if (GUILayout.Button ("play")) {
			Vungle.playAd(list[0]);
		}
		GUI.enabled = true;

		GUILayout.Label ("Placement 2");
		GUILayout.Label ("PlacementID " + list[1]);
		GUI.enabled = placements[list[1]];
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("play")) {
			Vungle.playAd(list[1]);
		}
		GUI.enabled = !placements[list[1]];
		if (GUILayout.Button ("load")) {
			Vungle.loadAd(list[1]);
		}
		GUILayout.EndHorizontal();
		GUI.enabled = true;

		GUILayout.Label ("Placement 3");
		GUILayout.Label ("PlacementID " + list[2]);
		GUI.enabled = placements[list[2]];
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("play")) {
			Vungle.playAd(list[2]);
		}
		GUI.enabled = !placements[list[2]];
		if (GUILayout.Button ("load")) {
			Vungle.loadAd(list[2]);
		}
		GUILayout.EndHorizontal();

		/*
		//Begin overall view layout

		GUILayout.FlexibleSpace ();

		//Vungle Header
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.Label (vungleLogo, titleLabelStyle, GUILayout.Height ((int)(headerHeight * .67)));
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

		GUILayout.FlexibleSpace ();

		//Only enable the buttons if we've got a cached ad ready to play
		GUI.enabled = Vungle.isAdvertAvailable();
		//Default PlayAd Button + onClick Handler
		if (GUILayout.Button (playDefaultAdTexture, imageButtonStyle, GUILayout.Height (buttonHeight))) {
			//Play default ad on click
			Vungle.playAd ();
		}

		//Incentivized Ad Button + onClick Handler
		if (GUILayout.Button (playIncentivizedAdTexture, imageButtonStyle, GUILayout.Height (buttonHeight))) {
			//Play Incentivized Ad on click
			Vungle.playAd(true, "example user name");
		}

		//Play Ad with All Options Button + onClick handler
		if (GUILayout.Button (playCustomAdTexture, imageButtonStyle, GUILayout.Height (buttonHeight))) {
			//Start the ad muted, unincentivized, and with a user info string
			Vungle.setSoundEnabled(false);
			Vungle.playAd(false, "a different user name");
		}
		GUI.enabled = true;
		*/
		
		GUILayout.EndArea ();
	}

	void initializeTextures() {

		whiteBackgroundTexture = singleColorTex (Screen.width, Screen.height, Color.white);

		vungleLogo = (Texture2D)Resources.Load ("VungleLogo");
		if (vungleLogo == null) {
			DebugLog("vungleLogo texture didn't load!");
		}
		playDefaultAdTexture = (Texture2D)Resources.Load ("PlayDefaultAdButton");
		if (playDefaultAdTexture == null) {
			DebugLog("defaultAdButton texture didn't load!");
		}
		playIncentivizedAdTexture = (Texture2D)Resources.Load ("PlayIncentivizedAdButton");
		if (playIncentivizedAdTexture == null) {
			DebugLog("playIncentivizedAdTexture texture didn't load!");
		}
		playCustomAdTexture = (Texture2D)Resources.Load ("PlayCustomAdButton");
		if (playCustomAdTexture == null) {
			DebugLog("playCustomAdTexture texture didn't load!");
		}
	}

	void initializeGUIStyles() {
		imageButtonStyle = new GUIStyle ();
		imageButtonStyle.stretchHeight = true;
		imageButtonStyle.stretchWidth = true;
		imageButtonStyle.fixedWidth = Screen.width;
		imageButtonStyle.fixedHeight = buttonHeight;
		
		uiStyle = new GUIStyle ();

		titleLabelStyle = new GUIStyle ();
		titleLabelStyle.stretchWidth = true;
		titleLabelStyle.stretchHeight = true;
		titleLabelStyle.fixedWidth = Screen.width;
//		titleLabelStyle.fixedHeight = headerHeight;
		titleLabelStyle.alignment = TextAnchor.MiddleCenter;
	}
		
	void initializeUISizes () {
		headerHeight = (int)(Screen.height * 0.1);
		int adjustedScreenHeight = Screen.height - headerHeight - (2 * spacerHeight);
		buttonHeight = adjustedScreenHeight / buttonCount;
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

	}

	/* Basic method used for building out the background texture for the main interface */
	Texture2D singleColorTex(int width, int height, Color col) {
		Color[] pix = new Color[width * height];
		for (int i=0; i<pix.Length; i++) {
			pix[i] = col;
		}

		Texture2D tex = new Texture2D (width, height);
		tex.SetPixels (pix);
		tex.Apply ();

		return tex;
	}

	/* Common method for ensuring logging messages have the same format */
	void DebugLog(string message) {
		Debug.Log(logTag + System.DateTime.Today +": " + message);
	}
}
#endif
