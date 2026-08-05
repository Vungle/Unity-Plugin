using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class RewardedController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleRewarded rewardedAd;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateBackButton(content, () => { Log("Back tapped"); SceneManager.LoadScene("LaunchScene"); });
            UIHelper.CreateTitle("Rewarded", content);
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
            rewardedAd = new VungleRewarded(VungleConstants.RewardedPlacementId);
            SetupCallbacks();
            rewardedAd.Load();
            Log("Rewarded loading...");
    #else
            Log("Rewarded not supported in editor");
    #endif
        }

        void OnPresent()
        {
            Log("Present tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (rewardedAd != null)
            {
                rewardedAd.Show();
                Log("Rewarded present called");
            }
            else
            {
                Log("Rewarded not loaded");
            }
    #else
            Log("Rewarded not supported in editor");
    #endif
        }

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        void SetupCallbacks()
        {
            rewardedAd.onLoadSuccess += () => Log("Rewarded loaded");
            rewardedAd.onLoadFailed += (err) => Log("Rewarded load failed: " + err);
            rewardedAd.onDidPresent += () => Log("Rewarded presented");
            rewardedAd.onPresentFailed += (err) => Log("Rewarded present failed: " + err);
            rewardedAd.onDidClose += () => { Log("Rewarded closed"); rewardedAd = null; };
            rewardedAd.onDidRewardUser += () => Log("Rewarded user rewarded");
            rewardedAd.onImpression += () => Log("Rewarded impression");
            rewardedAd.onClick += () => Log("Rewarded click");
        }
    #endif
    }
}
