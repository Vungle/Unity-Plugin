using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BannerGUI : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO

#if UNITY_IPHONE
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "BANNER04-8166553", false },
		{ "MREC03-4489762", false },
		{ "BANNER05-4730786", false }
	};

#elif UNITY_ANDROID
	Dictionary<string, bool> placements = new Dictionary<string, bool> 
	{
		{ "BANNER-5454585", false },
		{ "MREC-2191415", false },
		{ "BANNER2-6990728", false }
	};
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
	Dictionary<string, bool> placements = new Dictionary<string, bool>
	{
		{ "BANNER01-2481140", false },
		{ "MREC01-4900382", false },
		{ "BANNER04-2625387", false }
	};
	
#endif

	public Button loadBanner1Button;
	public Button playBanner1Button;
	public Button loadMrec1Button;
	public Button playMrec1Button;
	public Button loadBanner2Button;
	public Button playBanner2Button;
	public Button closeBanner1Button;
	public Button closeMrec1Button;
	public Button closeBanner2Button;
	public Button toFullscreenButton;
	public Text placementID1Text;
	public Text placementID2Text;
	public Text placementID3Text;

	List<string> placementLists;

	bool adInited = true;

	// Use this for initialization
	void Start()
	{
		Setup();
	}

	// Update is called once per frame
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

	void Setup()
	{
		placementLists = new List<string>(placements.Keys);

		placementID1Text.text = "Placement ID: " + placementLists[0];
		placementID2Text.text = "Placement ID: " + placementLists[1];
		placementID3Text.text = "Placement ID: " + placementLists[2];

		toFullscreenButton.onClick.AddListener(showFullscreenCanvas);
		loadBanner1Button.onClick.AddListener(onLoadBanner1);
		loadMrec1Button.onClick.AddListener(onLoadMrec1);
		loadBanner2Button.onClick.AddListener(onLoadBanner2);
		playBanner1Button.onClick.AddListener(onPlayBanner1);
		playMrec1Button.onClick.AddListener(onPlayMrec1);
		playBanner2Button.onClick.AddListener(onPlayBanner2);
		closeBanner1Button.onClick.AddListener(onCloseBanner1);
		closeMrec1Button.onClick.AddListener(onCloseMrec1);
		closeBanner2Button.onClick.AddListener(onCloseBanner2);

		closeBanner1Button.interactable = false;
		closeMrec1Button.gameObject.SetActive(false);
		closeBanner2Button.interactable = false;

		initEventHandlers();
	}

	void onLoadBanner1()
	{
		Vungle.loadBanner(placementLists[0], Vungle.VungleBannerSize.VungleAdSizeBanner, Vungle.VungleBannerPosition.TopCenter);
	}

	void onPlayBanner1()
	{
		Vungle.showBanner(placementLists[0]);
		closeBanner1Button.interactable = true;
	}

	void onLoadMrec1()
	{
		Vungle.loadBanner(placementLists[1], Vungle.VungleBannerSize.VungleAdSizeBannerMedium, Vungle.VungleBannerPosition.Centered);
	}

	void onPlayMrec1()
	{
		Vungle.showBanner(placementLists[1]);
		closeMrec1Button.gameObject.SetActive(true);
	}

	void onLoadBanner2()
	{
		Vungle.loadBanner(placementLists[2], Vungle.VungleBannerSize.VungleAdSizeBannerShort, Vungle.VungleBannerPosition.BottomCenter);
	}

	void onPlayBanner2()
	{
		Vungle.showBanner(placementLists[2]);
		closeBanner2Button.interactable = true;
	}

	void onCloseBanner1()
	{
		Vungle.closeBanner(placementLists[0]);
		closeBanner1Button.interactable = false;
	}

	void onCloseMrec1()
	{
		Vungle.closeBanner(placementLists[1]);
		closeMrec1Button.gameObject.SetActive(false);
	}

	void onCloseBanner2()
	{
		Vungle.closeBanner(placementLists[2]);
		closeBanner2Button.interactable = false;
	}

	void showFullscreenCanvas()
	{
		ViewManager.Instance.LoadView(ViewManager.ViewId.FullAd);
		ViewManager.Instance.canvases[1].gameObject.SetActive(false);
	}

	void updateButtonState()
	{
		loadBanner1Button.interactable = !placements[placementLists[0]];
		playBanner1Button.interactable = placements[placementLists[0]];
		loadMrec1Button.interactable = !placements[placementLists[1]];
		playMrec1Button.interactable = placements[placementLists[1]];
		loadBanner2Button.interactable = !placements[placementLists[2]];
		playBanner2Button.interactable = placements[placementLists[2]];
	}

	void initEventHandlers()
	{
		// Event triggered during when an ad is about to be played
		Vungle.onAdStartedEvent += (placementID) =>
		{
			DebugLog("Ad " + placementID + " is starting!  Pause your game  animation or sound here.");
#if UNITY_ANDROID
				placements[placementID] = false;
#endif
		};

		// Event is triggered when a Vungle ad finished and provides the entire information about this event
		// These can be used to determine how much of the video the user viewed, if they skipped the ad early, etc.
		Vungle.onAdFinishedEvent += (placementID, args) =>
		{
			DebugLog("Ad finished - placementID " + placementID + ", was call to action clicked:" + args.WasCallToActionClicked + ", is completed view:"
				+ args.IsCompletedView);
			//updateButtonState();
			placements[placementID] = false;
		};

		// Event is triggered when the ad's playable state has been changed
		// It can be used to enable certain functionality only accessible when ad plays are available
		Vungle.adPlayableEvent += (placementID, adPlayable) =>
		{
			DebugLog("Banner Ad's playable state has been changed! placementID " + placementID + ". Now: " + adPlayable);
			placements[placementID] = adPlayable;
		};

		Vungle.onAdClickEvent += (placementID) => 
		{
			DebugLog("onClick - Log: " + placementID);
		};

		Vungle.onAdRewardedEvent += (placementID) => 
		{
			DebugLog("onAdRewardedEvent - Log: " + placementID);
		};

		Vungle.onAdEndEvent += (placementID) => 
		{
			DebugLog("onAdEnd - Log: " + placementID); 
		};
	}

	void DebugLog(string message)
	{
		Debug.Log("VungleUnity" + System.DateTime.Now + ": " + message);
	}

#endif
}

