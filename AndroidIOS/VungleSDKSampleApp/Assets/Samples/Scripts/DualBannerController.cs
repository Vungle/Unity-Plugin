using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class DualBannerController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;
        LayoutElement topSpacer, bottomSpacer;

        static readonly Color BackColor = new Color(0.38f, 0.38f, 0.43f);

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleBannerView topBanner, bottomBanner;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            var topSpacerObj = new GameObject("TopBannerSpacer");
            topSpacerObj.transform.SetParent(content, false);
            topSpacer = topSpacerObj.AddComponent<LayoutElement>();

            UIHelper.CreateBackButton(content, OnBack);
            UIHelper.CreateTitle("Dual Banner", content);
            UIHelper.CreateSpacer(content, 8);

            UIHelper.CreateButton("Load Both", content, OnLoadBoth);
            UIHelper.CreateButton("Attach Both", content, OnAttachBoth);
            UIHelper.CreateButton("Destroy Both", content, OnDestroyBoth);

            UIHelper.CreateSpacer(content, 8);
            UIHelper.CreateLabel("Log", content, 22, FontStyles.Bold);
            var (lt, sr) = UIHelper.CreateLogArea(content);
            logText = lt;
            logScroll = sr;
            UIHelper.CreateButton("Clear Log", content, () => VungleTestManager.Instance.ClearLog(),
                50, BackColor);

            var bottomSpacerObj = new GameObject("BottomBannerSpacer");
            bottomSpacerObj.transform.SetParent(content, false);
            bottomSpacer = bottomSpacerObj.AddComponent<LayoutElement>();

            VungleTestManager.Instance.OnLogChanged += MarkLogDirty;
            logDirty = true;
        }

        void OnDestroy()
        {
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            topBanner?.Destroy();
            topBanner = null;
            bottomBanner?.Destroy();
            bottomBanner = null;
    #endif
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

        void OnBack()
        {
            Log("Back tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            topBanner?.Destroy();
            topBanner = null;
            bottomBanner?.Destroy();
            bottomBanner = null;
    #endif
            SceneManager.LoadScene("LaunchScene");
        }

        void OnLoadBoth()
        {
            Log("Load Both tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            topBanner?.Destroy();
            bottomBanner?.Destroy();

            string placementId = VungleConstants.BannerPlacementId;

            topBanner = new VungleBannerView(placementId, VungleBannerSize.Banner);
            topBanner.onLoadSuccess += () =>
            {
                Log("Top banner loaded");
                topBanner.Attach(0, -10000, 1, 1);
            };
            topBanner.onLoadFailed += (err) => Log("Top banner load failed: " + err);
            topBanner.onDidPresent += () => Log("Top banner presented");
            topBanner.onImpression += () => Log("Top banner impression");
            topBanner.onClick += () => Log("Top banner click");
            topBanner.onDidClose += () => { Log("Top banner closed"); topBanner = null; };
            topBanner.Load();
            Log("Top banner loading...");

            bottomBanner = new VungleBannerView(placementId, VungleBannerSize.Banner);
            bottomBanner.onLoadSuccess += () =>
            {
                Log("Bottom banner loaded");
                bottomBanner.Attach(0, -10000, 1, 1);
            };
            bottomBanner.onLoadFailed += (err) => Log("Bottom banner load failed: " + err);
            bottomBanner.onDidPresent += () => Log("Bottom banner presented");
            bottomBanner.onImpression += () => Log("Bottom banner impression");
            bottomBanner.onClick += () => Log("Bottom banner click");
            bottomBanner.onDidClose += () => { Log("Bottom banner closed"); bottomBanner = null; };
            bottomBanner.Load();
            Log("Bottom banner loading...");
    #else
            Log("Banner not supported in editor");
    #endif
        }

        void OnAttachBoth()
        {
            Log("Attach Both tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            int bannerPxH = GetBannerPixelHeight();
            int bannerPxW = GetBannerPixelWidth();
            int x = (Screen.width - bannerPxW) / 2;
            float canvasScale = logText.canvas != null ? logText.canvas.scaleFactor : 1f;

            if (topBanner != null)
            {
                topBanner.Attach(x, 0, bannerPxW, bannerPxH);
                Log("Top banner attached at y=0");
                topSpacer.minHeight = bannerPxH / canvasScale + 10;
                topSpacer.preferredHeight = topSpacer.minHeight;
            }
            else
            {
                Log("Top banner not loaded");
            }

            if (bottomBanner != null)
            {
                int y = Screen.height - bannerPxH;
                bottomBanner.Attach(x, y, bannerPxW, bannerPxH);
                Log($"Bottom banner attached at y={y}");
                bottomSpacer.minHeight = bannerPxH / canvasScale + 10;
                bottomSpacer.preferredHeight = bottomSpacer.minHeight;
            }
            else
            {
                Log("Bottom banner not loaded");
            }
    #else
            Log("Banner not supported in editor");
    #endif
        }

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        int GetBannerPixelHeight()
        {
            float density = Screen.dpi > 0 ? Screen.dpi / 160f : 2f;
            return (int)(50 * density);
        }

        int GetBannerPixelWidth()
        {
            float density = Screen.dpi > 0 ? Screen.dpi / 160f : 2f;
            return (int)(320 * density);
        }
    #endif

        void OnDestroyBoth()
        {
            Log("Destroy Both tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            topBanner?.Destroy();
            topBanner = null;
            bottomBanner?.Destroy();
            bottomBanner = null;
            Log("Both banners destroyed");
    #else
            Log("Banner not supported in editor");
    #endif
            topSpacer.minHeight = 0;
            topSpacer.preferredHeight = 0;
            bottomSpacer.minHeight = 0;
            bottomSpacer.preferredHeight = 0;
        }
    }
}
