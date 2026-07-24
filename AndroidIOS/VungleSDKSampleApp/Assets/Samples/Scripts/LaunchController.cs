using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace VungleAds.Samples
{
    public class LaunchController : MonoBehaviour
    {
        TMP_Text statusLabel;
        Button interstitialBtn, rewardedBtn, bannerBtn, nativeBtn, dualBannerBtn, dualNativeBtn, bannerRectTransformBtn;
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateTitle("Vungle SDK Test", content);
            UIHelper.CreateSpacer(content, 4);

            UIHelper.CreateButton("Initialize SDK", content, OnInitialize);
            statusLabel = UIHelper.CreateLabel("Status: Not Initialized", content);
            UIHelper.CreateSpacer(content, 8);

            interstitialBtn = UIHelper.CreateButton("Interstitial", content,
                () => { Log("Interstitial tapped"); SceneManager.LoadScene("InterstitialScene"); });
            rewardedBtn = UIHelper.CreateButton("Rewarded", content,
                () => { Log("Rewarded tapped"); SceneManager.LoadScene("RewardedScene"); });
            bannerBtn = UIHelper.CreateButton("Banner", content,
                () => { Log("Banner tapped"); SceneManager.LoadScene("BannerScene"); });
            nativeBtn = UIHelper.CreateButton("Native", content,
                () => { Log("Native tapped"); SceneManager.LoadScene("NativeScene"); });
            dualBannerBtn = UIHelper.CreateButton("Dual Banner", content,
                () => { Log("Dual Banner tapped"); SceneManager.LoadScene("DualBanner"); });
            dualNativeBtn = UIHelper.CreateButton("Dual Native", content,
                () => { Log("Dual Native tapped"); SceneManager.LoadScene("DualNative"); });
            bannerRectTransformBtn = UIHelper.CreateButton("Banner RectTransform", content,
                () => { Log("Banner RectTransform tapped"); SceneManager.LoadScene("BannerRectTransformScene"); });

            UIHelper.CreateSpacer(content, 8);
            UIHelper.CreateLabel("Log", content, 22, FontStyles.Bold);
            var (lt, sr) = UIHelper.CreateLogArea(content);
            logText = lt;
            logScroll = sr;
            UIHelper.CreateButton("Clear Log", content, () => VungleTestManager.Instance.ClearLog(),
                50, new Color(0.38f, 0.38f, 0.43f));

            UpdateButtonStates();
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
                UpdateButtonStates();
                logText.text = VungleTestManager.Instance.LogText;
                StartCoroutine(ScrollToBottom());
            }
        }

        IEnumerator ScrollToBottom()
        {
            yield return null;
            logScroll.verticalNormalizedPosition = 0f;
        }

        void UpdateButtonStates()
        {
            bool init = VungleTestManager.Instance.SdkInitialized;
            statusLabel.text = init ? "Status: Initialized" : "Status: Not Initialized";
            statusLabel.color = init ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.7f, 0.4f);
            interstitialBtn.interactable = init;
            rewardedBtn.interactable = init;
            bannerBtn.interactable = init;
            nativeBtn.interactable = init;
            dualBannerBtn.interactable = init;
            dualNativeBtn.interactable = init;
            bannerRectTransformBtn.interactable = init;
        }

        void Log(string msg) => VungleTestManager.Instance.Log(msg);

        void OnInitialize()
        {
            Log("Initialize SDK tapped");
            VungleTestManager.Instance.InitializeSdk();
        }
    }
}
