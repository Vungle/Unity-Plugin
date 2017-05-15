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

	GUIStyle titleLabelStyle;
	bool adInited = false;
	void Start () {
		DebugLog("Initializing the Vungle SDK");
		//Initialize Everything
		initializeGUIStyles ();
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
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		List<string> list = new List<string>(placements.Keys);

		GUI.enabled = true;

		GUILayout.Label ("AppID " + appID, titleLabelStyle);
		if (GUILayout.Button ("Init SDK")) {
			string[] array = new string[placements.Keys.Count];
			placements.Keys.CopyTo(array, 0);
			Vungle.init (appID, appID, appID, array);
			initializeEventHandlers ();
		}

		GUI.enabled = adInited;
		GUILayout.Label ("Placement 1");
		GUILayout.Label ("PlacementID " + list[0]);
		GUI.enabled = placements[list[0]];
		if (GUILayout.Button ("play")) {
			Vungle.playAd(list[0]);
		}
		GUI.enabled = adInited;

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
		GUI.enabled = adInited;

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

		GUILayout.EndArea ();
	}

	void initializeGUIStyles() {
		titleLabelStyle = new GUIStyle ();
		titleLabelStyle.stretchWidth = true;
//		titleLabelStyle.stretchHeight = true;
		titleLabelStyle.fixedWidth = Screen.width;
//		titleLabelStyle.fixedHeight = headerHeight;
		titleLabelStyle.alignment = TextAnchor.MiddleCenter;
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
			adInited = true;
			DebugLog ("Ad's playable state has been changed! placementID " + placementID + ". Now: " + adPlayable);
			placements[placementID] = adPlayable;
		};

		//Fired log event from sdk
		Vungle.onLogEvent += (log) => {
			DebugLog ("Log: " + log);
		};

	}

	/* Common method for ensuring logging messages have the same format */
	void DebugLog(string message) {
		Debug.Log("VungleUnitySample " + System.DateTime.Today +": " + message);
	}
}
#endif
