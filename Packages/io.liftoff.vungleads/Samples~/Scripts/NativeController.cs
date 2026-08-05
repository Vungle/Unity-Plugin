using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class NativeController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;
        TMP_Text titleLabel, ratingLabel, bodyLabel, ctaLabel;
        RawImage iconImage;
        RectTransform mediaSlot;
        RectTransform adCardTop, adCardBottom;
        RectTransform iconRT, titleColRT, bodyRT, ctaBtnRT;
        Transform canvasRoot;
        GameObject configDialog;
        bool attached;

        // CTA configuration: which ad elements are clickable (media always is)
        bool clickIcon = true, clickTitle = true, clickBody = true, clickCta = true;

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleNative nativeAd;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateBackButton(content, () => { Log("Back tapped"); SceneManager.LoadScene("LaunchScene"); });
            UIHelper.CreateTitle("Native", content);
            UIHelper.CreateSpacer(content, 8);

            canvasRoot = content.GetComponentInParent<Canvas>().transform;

            UIHelper.CreateButton("Load", content, OnLoad);
            UIHelper.CreateButton("Attach", content, OnAttach);
            UIHelper.CreateButton("Detach", content, OnDetach);
            UIHelper.CreateButton("CTA Configuration", content, OnCtaConfiguration,
                70, new Color(0.55f, 0.4f, 0.85f));

            UIHelper.CreateSpacer(content, 12);

            // --- Ad card (matches native ad reference layout) ---

            // Header row: icon + title/rating
            adCardTop = CreateAdHeader(content);

            // MediaView placeholder (native overlay renders on top of this)
            mediaSlot = CreateMediaSlot(content);

            // Body text
            bodyLabel = UIHelper.CreateLabel("", content, 24, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 50);
            bodyRT = (RectTransform)bodyLabel.transform;

            // CTA button (green, left-aligned)
            ctaLabel = CreateCtaButton(content);

            // --- End ad card ---

            UIHelper.CreateSpacer(content, 8);
            UIHelper.CreateLabel("Log", content, 22, FontStyles.Bold);
            var (lt, sr) = UIHelper.CreateLogArea(content);
            logText = lt;
            logScroll = sr;
            UIHelper.CreateButton("Clear Log", content, () => VungleTestManager.Instance.ClearLog(),
                50, new Color(0.38f, 0.38f, 0.43f));

            VungleTestManager.Instance.OnLogChanged += MarkLogDirty;
            logDirty = true;
        }

        RectTransform CreateAdHeader(Transform parent)
        {
            var row = UIHelper.CreateHorizontalGroup(parent, 110, 16);

            // Icon frame (fixed width in HLG)
            var iconFrame = new GameObject("IconFrame");
            iconFrame.transform.SetParent(row, false);
            var iconLE = iconFrame.AddComponent<LayoutElement>();
            iconLE.minWidth = 110;
            iconLE.preferredWidth = 110;
            iconLE.flexibleWidth = 0;
            iconRT = (RectTransform)iconFrame.transform;

            // Icon image inside frame with aspect ratio fitter
            var iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(iconFrame.transform, false);
            var iconImgRT = (RectTransform)iconObj.transform;
            iconImgRT.anchorMin = Vector2.zero;
            iconImgRT.anchorMax = Vector2.one;
            iconImgRT.offsetMin = Vector2.zero;
            iconImgRT.offsetMax = Vector2.zero;
            iconImage = iconObj.AddComponent<RawImage>();
            iconImage.color = new Color(0.2f, 0.2f, 0.25f);
            var iconFitter = iconObj.AddComponent<AspectRatioFitter>();
            iconFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            iconFitter.aspectRatio = 1f;

            // Text column (title + rating)
            var textCol = new GameObject("InfoCol");
            textCol.transform.SetParent(row, false);
            var textLE = textCol.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;
            var vlg = textCol.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.MiddleLeft;

            titleLabel = AddText("--", textCol.transform, 28, FontStyles.Bold, Color.white);
            ratingLabel = AddText("Rating: --", textCol.transform, 20, FontStyles.Normal,
                new Color(0.7f, 0.7f, 0.7f));
            titleColRT = (RectTransform)textCol.transform;
            return row;
        }

        RectTransform CreateMediaSlot(Transform parent)
        {
            var obj = new GameObject("MediaSlot");
            obj.transform.SetParent(parent, false);
            var le = obj.AddComponent<LayoutElement>();
            le.minHeight = 500;
            le.preferredHeight = 500;
            obj.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
            return (RectTransform)obj.transform;
        }

        TMP_Text CreateCtaButton(Transform parent)
        {
            var row = new GameObject("CtaRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.minHeight = 60;
            rowLE.preferredHeight = 60;
            adCardBottom = (RectTransform)row.transform;

            var btnObj = new GameObject("CtaBtn", typeof(RectTransform));
            btnObj.transform.SetParent(row.transform, false);
            var btnRT = (RectTransform)btnObj.transform;
            btnRT.anchorMin = new Vector2(0, 0);
            btnRT.anchorMax = new Vector2(0, 1);
            btnRT.pivot = new Vector2(0, 0.5f);
            btnRT.sizeDelta = new Vector2(220, 0);
            btnRT.anchoredPosition = Vector2.zero;
            btnObj.AddComponent<Image>().color = new Color(0.18f, 0.72f, 0.33f);
            ctaBtnRT = btnRT;

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(btnObj.transform, false);
            var textRT = (RectTransform)textObj.transform;
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var label = textObj.AddComponent<TextMeshProUGUI>();
            label.text = "";
            label.fontSize = 24;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        TMP_Text AddText(string text, Transform parent, float fontSize, FontStyles style, Color color)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            var t = obj.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            return t;
        }

        void OnDestroy()
        {
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            nativeAd?.Destroy();
            nativeAd = null;
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

        void UpdateAdDisplay(string title, string body, string cta, double rating, string iconUrl)
        {
            titleLabel.text = string.IsNullOrEmpty(title) ? "--" : title;
            ratingLabel.text = "Rating: " + (rating > 0 ? rating.ToString("F1") : "0.0");
            bodyLabel.text = string.IsNullOrEmpty(body) ? "" : body;
            ctaLabel.text = string.IsNullOrEmpty(cta) ? "" : cta;
            if (!string.IsNullOrEmpty(iconUrl))
                StartCoroutine(LoadIcon(iconUrl));
        }

        IEnumerator LoadIcon(string url)
        {
            yield return UIHelper.LoadImageFromPath(url, iconImage);
            if (iconImage.texture != null)
            {
                var fitter = iconImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                    fitter.aspectRatio = (float)iconImage.texture.width / iconImage.texture.height;
            }
        }

        void ClearAdDisplay()
        {
            titleLabel.text = "--";
            ratingLabel.text = "Rating: --";
            bodyLabel.text = "";
            ctaLabel.text = "";
            iconImage.texture = null;
            iconImage.color = new Color(0.2f, 0.2f, 0.25f);
            var fitter = iconImage.GetComponent<AspectRatioFitter>();
            if (fitter != null) fitter.aspectRatio = 1f;
        }

        void OnLoad()
        {
            Log("Load tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            // Destroy the previous ad so its native views are unregistered
            // and released before a new one takes over
            if (nativeAd != null)
            {
                nativeAd.Destroy();
                attached = false;
                ClearAdDisplay();
                Log("Destroyed previous native ad");
            }
            nativeAd = new VungleNative(VungleConstants.NativePlacementId);
            SetupCallbacks();
            nativeAd.Load();
            Log("Native loading...");
    #else
            Log("Native not supported in editor");
    #endif
        }

        void OnAttach()
        {
            Log("Attach tapped");
            DoAttach();
        }

        // Attach the native container over the whole ad card (header through
        // CTA), with the MediaView laid out over the mediaSlot placeholder.
        // The elements enabled in the CTA Configuration dialog are registered
        // as clickable regions; the media view is always clickable.
        void DoAttach()
        {
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (nativeAd == null)
            {
                Log("No native ad loaded");
                return;
            }

            Canvas canvas = mediaSlot.GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            var (mediaMin, mediaMax) = GetScreenRect(mediaSlot, cam);
            var (topMin, topMax) = GetScreenRect(adCardTop, cam);
            var (bottomMin, bottomMax) = GetScreenRect(adCardBottom, cam);

            Vector2 cardMin = Vector2.Min(Vector2.Min(topMin, bottomMin), mediaMin);
            Vector2 cardMax = Vector2.Max(Vector2.Max(topMax, bottomMax), mediaMax);

            int x = (int)cardMin.x;
            int y = (int)(Screen.height - cardMax.y);
            int w = (int)(cardMax.x - cardMin.x);
            int h = (int)(cardMax.y - cardMin.y);
            int mx = (int)mediaMin.x;
            int my = (int)(Screen.height - mediaMax.y);
            int mw = (int)(mediaMax.x - mediaMin.x);
            int mh = (int)(mediaMax.y - mediaMin.y);

            var clickableRects = new System.Collections.Generic.List<RectInt>();
            var clickableNames = new System.Collections.Generic.List<string>();
            void AddClickable(bool enabled, RectTransform rt, string name)
            {
                if (!enabled) return;
                var (rmin, rmax) = GetScreenRect(rt, cam);
                clickableRects.Add(ToScreenRectInt(rmin, rmax));
                clickableNames.Add(name);
            }
            // The SDK only makes the media view clickable by default when NO
            // clickable list is given, so this sample always includes it.
            // Publishers can leave it out to make media not clickable.
            AddClickable(true, mediaSlot, "media");
            AddClickable(clickIcon, iconRT, "icon");
            AddClickable(clickTitle, titleColRT, "title");
            AddClickable(clickBody, bodyRT, "body");
            AddClickable(clickCta, ctaBtnRT, "CTA");

            nativeAd.Attach(x, y, w, h, mx, my, mw, mh,
                clickableRects.Count > 0 ? clickableRects.ToArray() : null);
            attached = true;
            UpdateAdDisplay(nativeAd.AdTitle, nativeAd.AdBody, nativeAd.AdCallToAction,
                nativeAd.AdStarRating, nativeAd.AdIconUrl);
            Log($"Native attached at ({x},{y} {w}x{h}), clickable: {string.Join(", ", clickableNames)}");
    #else
            Log("Native not supported in editor");
    #endif
        }

        void OnDetach()
        {
            Log("Detach tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (nativeAd == null) return;
            nativeAd.Detach();
            attached = false;
            ClearAdDisplay();
            Log("Native detached");
    #else
            Log("Native not supported in editor");
    #endif
        }

        // --- CTA Configuration dialog ---

        void OnCtaConfiguration()
        {
            Log("CTA Configuration tapped");

            // The native overlay renders above all Unity UI, so hide it while
            // the dialog is open; Done re-attaches with the new configuration.
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (attached && nativeAd != null)
                nativeAd.Detach();
    #endif

            if (configDialog != null)
            {
                configDialog.SetActive(true);
                return;
            }

            // Dim overlay blocking the scene behind the dialog
            configDialog = new GameObject("CtaConfigDialog", typeof(RectTransform));
            configDialog.transform.SetParent(canvasRoot, false);
            var overlayRT = (RectTransform)configDialog.transform;
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            configDialog.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // Centered panel
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(configDialog.transform, false);
            var panelRT = (RectTransform)panel.transform;
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(640, 0);
            panel.AddComponent<Image>().color = new Color(0.16f, 0.16f, 0.22f);
            var fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            UIHelper.CreateLabel("CTA Configuration", panel.transform, 30, FontStyles.Bold);
            UIHelper.CreateLabel("Select which ad elements are clickable.\nThis sample always includes the media view.",
                panel.transform, 20, FontStyles.Normal, TextAlignmentOptions.Center, 60);
            UIHelper.CreateSpacer(panel.transform, 4);

            CreateToggleButton(panel.transform, "Icon", () => clickIcon, v => clickIcon = v);
            CreateToggleButton(panel.transform, "Title / Rating", () => clickTitle, v => clickTitle = v);
            CreateToggleButton(panel.transform, "Body Text", () => clickBody, v => clickBody = v);
            CreateToggleButton(panel.transform, "CTA Button", () => clickCta, v => clickCta = v);

            UIHelper.CreateSpacer(panel.transform, 4);
            UIHelper.CreateButton("Done", panel.transform, () =>
            {
                configDialog.SetActive(false);
                Log($"CTA config: icon={clickIcon}, title={clickTitle}, body={clickBody}, cta={clickCta}");
                if (attached)
                {
                    Log("Re-attaching with new CTA configuration");
                    DoAttach();
                }
            }, 60);

            // Ensure the dialog renders above everything else in the canvas
            configDialog.transform.SetAsLastSibling();
        }

        void CreateToggleButton(Transform parent, string label,
            System.Func<bool> get, System.Action<bool> set)
        {
            Button btn = null;
            btn = UIHelper.CreateButton("", parent, () =>
            {
                set(!get());
                RefreshToggleButton(btn, label, get());
            }, 60);
            RefreshToggleButton(btn, label, get());
        }

        void RefreshToggleButton(Button btn, string label, bool on)
        {
            btn.GetComponent<Image>().color = on
                ? new Color(0.18f, 0.72f, 0.33f)
                : new Color(0.38f, 0.38f, 0.43f);
            btn.GetComponentInChildren<TMP_Text>().text = $"{label}: {(on ? "Clickable" : "Not clickable")}";
        }

        (Vector2 min, Vector2 max) GetScreenRect(RectTransform rt, Camera cam)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
            return (min, max);
        }

        RectInt ToScreenRectInt(Vector2 min, Vector2 max)
        {
            return new RectInt((int)min.x, (int)(Screen.height - max.y),
                (int)(max.x - min.x), (int)(max.y - min.y));
        }

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        void SetupCallbacks()
        {
            nativeAd.onLoadSuccess += () => Log("Native loaded");
            nativeAd.onLoadFailed += (err) => Log("Native load failed: " + err);
            nativeAd.onDidPresent += () => Log("Native presented");
            nativeAd.onPresentFailed += (err) => Log("Native present failed: " + err);
            nativeAd.onDidClose += () => { Log("Native closed"); nativeAd = null; attached = false; };
            nativeAd.onImpression += () => Log("Native impression");
            nativeAd.onClick += () => Log("Native click");
            nativeAd.onAdDataReceived += (title, body, cta, rating, iconUrl) =>
            {
                Log($"Native data: title={title}, cta={cta}, rating={rating}");
            };
        }
    #endif
    }
}
