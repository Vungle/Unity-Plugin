using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class InterstitialController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleInterstitial interstitialAd;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateBackButton(content, () => { Log("Back tapped"); SceneManager.LoadScene("LaunchScene"); });
            UIHelper.CreateTitle("Interstitial", content);
            UIHelper.CreateSpacer(content, 8);

            UIHelper.CreateButton("Load", content, OnLoad);
            UIHelper.CreateSpacer(content, 4);
            UIHelper.CreateButton("Present", content, OnPresent);

            UIHelper.CreateSpacer(content, 12);
            UIHelper.CreateLabel("Log", content, 22, FontStyles.Bold);
            var (lt, sr) = UIHelper.CreateLogArea(content);
            logText = lt;
            logScroll = sr;
            UIHelper.CreateButton("Clear Log", content, () => VungleTestManager.Instance.ClearLog(),
                50, new Color(0.38f, 0.38f, 0.43f));

            VungleTestManager.Instance.OnLogChanged += MarkLogDirty;
            logDirty = true;
        }

        void OnDestroy()
        {
            if (VungleTestManager.HasInstance)
                VungleTestManager.Instance.OnLogChanged -= MarkLogDirty;
        }

        void MarkLogDirty() => logDirty = true;

        void Update()
        {
            if (logDirty)
            {
                logDirty = false;
                logText.text = VungleTestManager.Instance.LogText;
                StartCoroutine(ScrollToBottom());
            }
        }

        IEnumerator ScrollToBottom()
        {
            yield return null;
            logScroll.verticalNormalizedPosition = 0f;
        }

        void Log(string msg) => VungleTestManager.Instance.Log(msg);

        void OnLoad()
        {
            Log("Load tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            interstitialAd = new VungleInterstitial(VungleConstants.InterstitialPlacementId);
            SetupCallbacks();
            interstitialAd.Load();
            Log("Interstitial loading...");
    #else
            Log("Interstitial not supported in editor");
    #endif
        }

        void OnPresent()
        {
            Log("Present tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (interstitialAd != null)
            {
                interstitialAd.Show();
                Log("Interstitial present called");
            }
            else
            {
                Log("Interstitial not loaded");
            }
    #else
            Log("Interstitial not supported in editor");
    #endif
        }

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        void SetupCallbacks()
        {
            interstitialAd.onLoadSuccess += () => Log("Interstitial loaded");
            interstitialAd.onLoadFailed += (err) => Log("Interstitial load failed: " + err);
            interstitialAd.onDidPresent += () => Log("Interstitial presented");
            interstitialAd.onPresentFailed += (err) => Log("Interstitial present failed: " + err);
            interstitialAd.onDidClose += () => { Log("Interstitial closed"); interstitialAd = null; };
            interstitialAd.onImpression += () => Log("Interstitial impression");
            interstitialAd.onClick += () => Log("Interstitial click");
        }
    #endif
    }
}
