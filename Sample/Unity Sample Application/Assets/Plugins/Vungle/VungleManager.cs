using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 618

public class VungleManager : MonoBehaviour
{
	private static AdFinishedEventArgs adWinFinishedEventArgs = null;

	#region Constructor and Lifecycle

	static VungleManager()
	{
		// try/catch this so that we can warn users if they try to stick this script on a GO manually
		try
		{
			// create a new GO for our manager
			var go = new GameObject( "VungleManager" );
			go.AddComponent<VungleManager>();
			DontDestroyOnLoad( go );
		}
		catch( UnityException )
		{
			Debug.LogWarning( "It looks like you have the VungleManager on a GameObject in your scene. Please remove the script from your scene." );
		}
	}


	// used to ensure the VungleManager will always be in the scene to avoid SendMessage logs if the user isn't using any events
	public static void noop(){}

	#endregion
	
	// Fired when the video is shown
	public static event Action<string> OnAdStartEvent;

	// Fired when a Vungle ad is ready to be displayed
	public static event Action<string, bool> OnAdPlayableEvent;

	// Fired when a Vungle write log (implemented only for iOS)
	public static event Action<string> OnSDKLogEvent;

	// Fired when a Vungle Placement Prepared (implemented only for iOS)
	public static event Action<string, string> OnPlacementPreparedEvent;

	// Fired when a Vungle Creative fired (implemented only for iOS)
	public static event Action<string, string> OnVungleCreativeEvent;

	//Fired when a Vungle ad finished and provides the entire information about this event.
	public static event Action<string, AdFinishedEventArgs> OnAdFinishedEvent;

	// Fired when a Vungle SDK initialized
	public static event Action OnSDKInitializeEvent;

	public static void onEvent(string e, string arg) {
		if (e == "OnAdStart") {
			OnAdStartEvent(arg);
		}
		if (e == "OnAdEnd") {
			adWinFinishedEventArgs = new AdFinishedEventArgs();
			
			var args = arg.Split(new char[] { ':' });
			adWinFinishedEventArgs.WasCallToActionClicked = "1".Equals (args[0]);
			adWinFinishedEventArgs.IsCompletedView = bool.Parse(args[2]);
			adWinFinishedEventArgs.TimeWatched = double.Parse(args[3]) / 1000;

			OnAdFinishedEvent(args[1], adWinFinishedEventArgs);
		}
		if (e == "OnAdPlayableChanged") {
			var args = arg.Split(new char[] { ':' });
			OnAdPlayableEvent(args[1], "1".Equals (args[0]));
		}
		if (e == "Diagnostic") {
			OnSDKLogEvent(arg);
		}
        if(e == "OnInitCompleted")
        {
            if("1".Equals(arg))
                OnSDKInitializeEvent();
        }
	}

	#region Native code will call these methods

	//methods for ios and andriod platforms
	void OnAdStart(string placementID) {
		OnAdStartEvent(placementID);
	}

	void OnAdPlayable(string param)
	{
		Dictionary<string,object> attrs = (Dictionary<string,object>) MiniJSONV.Json.Deserialize(param);
		bool isAdAvailable = extractBoolValue(attrs,"isAdAvailable");
		string placementID = attrs["placementID"].ToString();
		OnAdPlayableEvent(placementID, isAdAvailable);
	}

	void OnVideoView(string param) {
		#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
		
		#endif
	}

	void OnSDKLog(string log)
	{
		OnSDKLogEvent(log);
	}

	void OnPlacementPrepared(string param)
	{
		Dictionary<string,object> attrs = (Dictionary<string,object>) MiniJSONV.Json.Deserialize(param);
		string placementID = attrs["placementID"].ToString();
		string bidToken = attrs["bidToken"].ToString();
		OnPlacementPreparedEvent(placementID, bidToken);
	}

	void OnVungleCreative(string param)
	{
		Dictionary<string,object> attrs = (Dictionary<string,object>) MiniJSONV.Json.Deserialize(param);
		string placementID = attrs["placementID"].ToString();
		string creativeID = attrs["creativeID"].ToString();
		OnVungleCreativeEvent(placementID, creativeID);
	}

	void OnInitialize(string empty)
	{
		OnSDKInitializeEvent();
	}
	
	//methods only for android
	void OnAdEnd(string param)
	{
		AdFinishedEventArgs args = new AdFinishedEventArgs();
		Dictionary<string,object> attrs = (Dictionary<string,object>) MiniJSONV.Json.Deserialize(param);
#if UNITY_ANDROID
		args.WasCallToActionClicked = extractBoolValue(attrs,"wasCallToActionClicked");
		args.IsCompletedView = extractBoolValue(attrs,"wasSuccessFulView");
		args.TimeWatched = 0.0;
#elif UNITY_IPHONE
		//param is the json string
		args.WasCallToActionClicked = extractBoolValue(attrs,"didDownload");
		args.IsCompletedView = extractBoolValue(attrs,"completedView");
		args.TimeWatched = double.Parse(attrs["playTime"].ToString());
#endif
		OnAdFinishedEvent(attrs["placementID"].ToString(), args);
	}
	
	#endregion

	#region util methods

	private bool extractBoolValue(string json, string key)
	{
		Dictionary<string,object> attrs = (Dictionary<string,object>)MiniJSONV.Json.Deserialize( json );
		return extractBoolValue (attrs, key);
	}

	private bool extractBoolValue(Dictionary<string,object> attrs, string key)
	{
		return bool.Parse( attrs[key].ToString() );
	}

	#endregion
}


