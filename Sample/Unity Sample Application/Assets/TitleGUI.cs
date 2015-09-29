using UnityEngine;
using System.Collections;

public class TitleGUI : MonoBehaviour {

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
		Vungle.init ("Test_Android", "Test_iOS");

		//Initialize Everything
		initializeTextures ();
		initializeGUIStyles ();
		initializeUISizes ();
		initializeEventHandlers ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI () {

		//Begin overall view layout
		uiStyle.normal.background = whiteBackgroundTexture;
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height), uiStyle);

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
		
		GUILayout.EndArea ();
	}

	/* Setup default textures (from static PNG images) to be used on the interface buttons */
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

	/* Setup the default GUIStyles used in the main interface */
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
		titleLabelStyle.fixedHeight = headerHeight;
		titleLabelStyle.alignment = TextAnchor.MiddleCenter;
	}
		
	/* Setup size constants used to organize and align main interface */
	void initializeUISizes () {
		headerHeight = (int)(Screen.height * 0.1);
		int adjustedScreenHeight = Screen.height - headerHeight - (2 * spacerHeight);
		buttonHeight = adjustedScreenHeight / buttonCount;
	}

	/* Setup EventHandlers for all available Vungle events */
	void initializeEventHandlers() {

		//Event triggered during when an ad is about to be played
		Vungle.onAdStartedEvent += () => {
			DebugLog ("Ad event is starting!  Pause your game  animation or sound here.");
		};

		//Event is triggered when an ad play has ended
		Vungle.onAdEndedEvent += () => {
			DebugLog ("Ad has ended");
		};

		//Event is triggered after video completion, passes in timeWatched and totalDuration parameters
		//These can be used to determine how much of the video the user viewed, if they skipped the ad early, etc.
		Vungle.onAdViewedEvent += (timeWatched, totalDuration) => {
			DebugLog ("Ad viewed - Total Watched Time:" + timeWatched + "/" + totalDuration);
		};

		//Event is triggered when an ad has been locally cached and is ready for viewing
		//It can be used to enable certain functionality only accessible when ad plays are available
		Vungle.onCachedAdAvailableEvent += () => {
			DebugLog ("An ad has been cached for viewing!");
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