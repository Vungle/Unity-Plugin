using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class BannerController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;
        int selectedSizeIndex;
        int selectedPositionIndex;
        Button[] sizeButtons;
        Button[] positionButtons;
        GameObject customSizeRow;
        TMP_InputField widthInput, heightInput;
        LayoutElement bannerSpacer;

        static readonly Color SelectedColor = new Color(0.2f, 0.55f, 0.95f);
        static readonly Color UnselectedColor = new Color(0.27f, 0.27f, 0.32f);
        static readonly Color BackColor = new Color(0.38f, 0.38f, 0.43f);

        readonly string[] sizeLabels = {
            "Banner (320x50)",
            "Banner Short (300x50)",
            "Leaderboard (728x90)",
            "MREC (300x250)",
            "Inline (Custom)"
        };

        readonly string[] positionLabels = { "Top", "Center", "Bottom" };

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleBannerView bannerAd;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateBackButton(content, OnBack);
            UIHelper.CreateTitle("Banner", content);

            UIHelper.CreateLabel("Select Size:", content, 24, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 35);

            sizeButtons = new Button[sizeLabels.Length];
            for (int i = 0; i < sizeLabels.Length; i++)
            {
                int idx = i;
                sizeButtons[i] = UIHelper.CreateButton(sizeLabels[i], content,
                    () => SelectSize(idx), 50, UnselectedColor);
            }

            var hGroup = UIHelper.CreateHorizontalGroup(content, 50);
            customSizeRow = hGroup.parent.gameObject;

            UIHelper.CreateLabel("W:", hGroup, 22, FontStyles.Normal, TextAlignmentOptions.MidlineRight, 35);
            widthInput = UIHelper.CreateInputField("320", hGroup, 35);
            widthInput.text = "320";
            UIHelper.CreateLabel("H:", hGroup, 22, FontStyles.Normal, TextAlignmentOptions.MidlineRight, 35);
            heightInput = UIHelper.CreateInputField("100", hGroup, 35);
            heightInput.text = "100";

            UIHelper.CreateSpacer(content, 4);
            UIHelper.CreateLabel("Position:", content, 24, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 35);

            positionButtons = new Button[positionLabels.Length];
            var posRow = UIHelper.CreateHorizontalGroup(content, 50);
            for (int i = 0; i < positionLabels.Length; i++)
            {
                int idx = i;
                positionButtons[i] = UIHelper.CreateButton(positionLabels[i], posRow,
                    () => SelectPosition(idx), 35, UnselectedColor);
            }

            UIHelper.CreateSpacer(content, 4);
            UIHelper.CreateButton("Load", content, OnLoad);
            UIHelper.CreateButton("Attach", content, OnAttach);
            UIHelper.CreateButton("Destroy", content, DestroyBanner);

            UIHelper.CreateSpacer(content, 4);
            UIHelper.CreateLabel("Log", content, 22, FontStyles.Bold);
            var (lt, sr) = UIHelper.CreateLogArea(content);
            logText = lt;
            logScroll = sr;
            UIHelper.CreateButton("Clear Log", content, () => VungleTestManager.Instance.ClearLog(),
                50, BackColor);

            var spacerObj = new GameObject("BannerSpacer");
            spacerObj.transform.SetParent(content, false);
            bannerSpacer = spacerObj.AddComponent<LayoutElement>();

            SelectSize(0);
            SelectPosition(2);

            VungleTestManager.Instance.OnLogChanged += MarkLogDirty;
            logDirty = true;
        }

        void OnDestroy()
        {
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            bannerAd?.Destroy();
            bannerAd = null;
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

        void SelectSize(int index)
        {
            selectedSizeIndex = index;
            for (int i = 0; i < sizeButtons.Length; i++)
            {
                var img = sizeButtons[i].GetComponent<Image>();
                img.color = i == index ? SelectedColor : UnselectedColor;
            }
            customSizeRow.SetActive(index == 4);
        }

        void SelectPosition(int index)
        {
            selectedPositionIndex = index;
            for (int i = 0; i < positionButtons.Length; i++)
            {
                var img = positionButtons[i].GetComponent<Image>();
                img.color = i == index ? SelectedColor : UnselectedColor;
            }
        }

        void Log(string msg) => VungleTestManager.Instance.Log(msg);

        void OnBack()
        {
            Log("Back tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            bannerAd?.Destroy();
            bannerAd = null;
    #endif
            SceneManager.LoadScene("LaunchScene");
        }

        void OnLoad()
        {
            Log("Load tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            bannerAd?.Destroy();
            bannerAd = null;

            if (selectedSizeIndex == 4)
            {
                int w = 320, h = 100;
                int.TryParse(widthInput.text, out w);
                int.TryParse(heightInput.text, out h);
                bannerAd = new VungleBannerView(VungleConstants.InlinePlacementId, w, h);
                Log($"Banner loading (inline {w}x{h})...");
            }
            else if (selectedSizeIndex == 3)
            {
                bannerAd = new VungleBannerView(VungleConstants.MrecPlacementId, VungleBannerSize.Mrec);
                Log($"Banner loading ({sizeLabels[selectedSizeIndex]})...");
            }
            else
            {
                VungleBannerSize[] sizes = {
                    VungleBannerSize.Banner,
                    VungleBannerSize.BannerShort,
                    VungleBannerSize.BannerLeaderboard
                };
                bannerAd = new VungleBannerView(VungleConstants.BannerPlacementId, sizes[selectedSizeIndex]);
                Log($"Banner loading ({sizeLabels[selectedSizeIndex]})...");
            }

            bannerAd.onLoadSuccess += () =>
            {
                Log("Banner loaded");
                bannerAd.Attach(0, -10000, 1, 1);
            };
            bannerAd.onLoadFailed += (err) => Log("Banner load failed: " + err);
            bannerAd.onDidPresent += () => Log("Banner presented");
            bannerAd.onPresentFailed += (err) => Log("Banner present failed: " + err);
            bannerAd.onDidClose += () => { Log("Banner closed"); bannerAd = null; };
            bannerAd.onImpression += () => Log("Banner impression");
            bannerAd.onClick += () => Log("Banner click");
            bannerAd.Load();
    #else
            Log("Banner not supported in editor");
    #endif
        }

        void OnAttach()
        {
            Log("Attach tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (bannerAd != null)
            {
                int bannerPxH = GetBannerPixelHeight();
                int y;
                switch (selectedPositionIndex)
                {
                    case 0: y = 0; break;
                    case 1: y = (Screen.height - bannerPxH) / 2; break;
                    default: y = Screen.height - bannerPxH; break;
                }

                int bannerPxW = GetBannerPixelWidth();
                int x = (Screen.width - bannerPxW) / 2;
                bannerAd.Attach(x, y, bannerPxW, bannerPxH);
                Log($"Banner attached at ({x}, {y}) - {positionLabels[selectedPositionIndex]}");

                if (selectedPositionIndex == 2)
                {
                    float canvasScale = logText.canvas != null ? logText.canvas.scaleFactor : 1f;
                    bannerSpacer.minHeight = bannerPxH / canvasScale + 10;
                    bannerSpacer.preferredHeight = bannerSpacer.minHeight;
                }
                else
                {
                    bannerSpacer.minHeight = 0;
                    bannerSpacer.preferredHeight = 0;
                }
            }
            else
            {
                Log("Banner not loaded");
            }
    #else
            Log("Banner not supported in editor");
    #endif
        }

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        int GetBannerPixelHeight()
        {
            int[] dpHeights = { 50, 50, 90, 250, 100 };
            int dpH = selectedSizeIndex == 4
                ? (int.TryParse(heightInput.text, out int h) ? h : 100)
                : dpHeights[selectedSizeIndex];
            float density = Screen.dpi > 0 ? Screen.dpi / 160f : 2f;
            return (int)(dpH * density);
        }

        int GetBannerPixelWidth()
        {
            int[] dpWidths = { 320, 300, 728, 300, 320 };
            int dpW = selectedSizeIndex == 4
                ? (int.TryParse(widthInput.text, out int w) ? w : 320)
                : dpWidths[selectedSizeIndex];
            float density = Screen.dpi > 0 ? Screen.dpi / 160f : 2f;
            return (int)(dpW * density);
        }
    #endif

        void DestroyBanner()
        {
            Log("Destroy tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            bannerAd?.Destroy();
            bannerAd = null;
            Log("Banner destroyed");
    #else
            Log("Banner not supported in editor");
    #endif
            bannerSpacer.minHeight = 0;
            bannerSpacer.preferredHeight = 0;
        }
    }
}
