using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class DualNativeController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;

        // Top ad card
        TMP_Text topTitle, topRating, topBody, topCta;
        RawImage topIcon;
        RectTransform topMediaSlot;

        // Bottom ad card
        TMP_Text bottomTitle, bottomRating, bottomBody, bottomCta;
        RawImage bottomIcon;
        RectTransform bottomMediaSlot;

        static readonly Color BackColor = new Color(0.38f, 0.38f, 0.43f);

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleNative topNative, bottomNative;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateBackButton(content, OnBack);
            UIHelper.CreateTitle("Dual Native", content);
            UIHelper.CreateSpacer(content, 8);

            UIHelper.CreateButton("Load Both", content, OnLoadBoth);
            UIHelper.CreateButton("Attach Both", content, OnAttachBoth);
            UIHelper.CreateButton("Detach Both", content, OnDetachBoth);

            UIHelper.CreateSpacer(content, 8);

            // Top ad card
            UIHelper.CreateLabel("Top Ad", content, 22, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 30);
            CreateAdCard(content, out topIcon, out topTitle, out topRating,
                out topMediaSlot, out topBody, out topCta, 300);

            UIHelper.CreateSpacer(content, 8);

            // Bottom ad card
            UIHelper.CreateLabel("Bottom Ad", content, 22, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 30);
            CreateAdCard(content, out bottomIcon, out bottomTitle, out bottomRating,
                out bottomMediaSlot, out bottomBody, out bottomCta, 300);

            UIHelper.CreateSpacer(content, 8);
            UIHelper.CreateLabel("Log", content, 22, FontStyles.Bold);
            var (lt, sr) = UIHelper.CreateLogArea(content);
            logText = lt;
            logScroll = sr;
            UIHelper.CreateButton("Clear Log", content, () => VungleTestManager.Instance.ClearLog(),
                50, BackColor);

            VungleTestManager.Instance.OnLogChanged += MarkLogDirty;
            logDirty = true;
        }

        void CreateAdCard(Transform parent, out RawImage icon, out TMP_Text title,
            out TMP_Text rating, out RectTransform media, out TMP_Text body,
            out TMP_Text cta, float mediaHeight)
        {
            // Header row: icon + title/rating
            var row = UIHelper.CreateHorizontalGroup(parent, 80, 12);

            var iconFrame = new GameObject("IconFrame");
            iconFrame.transform.SetParent(row, false);
            var iconLE = iconFrame.AddComponent<LayoutElement>();
            iconLE.minWidth = 80;
            iconLE.preferredWidth = 80;
            iconLE.flexibleWidth = 0;

            var iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(iconFrame.transform, false);
            var iconRT = (RectTransform)iconObj.transform;
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            icon = iconObj.AddComponent<RawImage>();
            icon.color = new Color(0.2f, 0.2f, 0.25f);
            var iconFitter = iconObj.AddComponent<AspectRatioFitter>();
            iconFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            iconFitter.aspectRatio = 1f;

            var textCol = new GameObject("InfoCol");
            textCol.transform.SetParent(row, false);
            var textLE = textCol.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;
            var vlg = textCol.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.MiddleLeft;

            title = AddText("--", textCol.transform, 24, FontStyles.Bold, Color.white);
            rating = AddText("Rating: --", textCol.transform, 16, FontStyles.Normal,
                new Color(0.7f, 0.7f, 0.7f));

            // Media slot
            var slotObj = new GameObject("MediaSlot");
            slotObj.transform.SetParent(parent, false);
            var slotLE = slotObj.AddComponent<LayoutElement>();
            slotLE.minHeight = mediaHeight;
            slotLE.preferredHeight = mediaHeight;
            slotObj.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
            media = (RectTransform)slotObj.transform;

            // Body
            body = UIHelper.CreateLabel("", parent, 20, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 35);

            // CTA
            cta = CreateCtaButton(parent);
        }

        TMP_Text CreateCtaButton(Transform parent)
        {
            var row = new GameObject("CtaRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.minHeight = 50;
            rowLE.preferredHeight = 50;

            var btnObj = new GameObject("CtaBtn", typeof(RectTransform));
            btnObj.transform.SetParent(row.transform, false);
            var btnRT = (RectTransform)btnObj.transform;
            btnRT.anchorMin = new Vector2(0, 0);
            btnRT.anchorMax = new Vector2(0, 1);
            btnRT.pivot = new Vector2(0, 0.5f);
            btnRT.sizeDelta = new Vector2(200, 0);
            btnRT.anchoredPosition = Vector2.zero;
            btnObj.AddComponent<Image>().color = new Color(0.18f, 0.72f, 0.33f);

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(btnObj.transform, false);
            var textRT = (RectTransform)textObj.transform;
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var label = textObj.AddComponent<TextMeshProUGUI>();
            label.text = "";
            label.fontSize = 20;
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
            topNative?.Destroy();
            topNative = null;
            bottomNative?.Destroy();
            bottomNative = null;
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
            SceneManager.LoadScene("LaunchScene");
        }

        // Attaches the native container over the whole ad card (header through
        // CTA) with every element clickable — a single clickable rect covering
        // the card. The MediaView is laid out over the media slot inside it.
        void AttachToSlot(VungleNative native, RectTransform mediaSlot,
            RawImage icon, TMP_Text cta, string label)
        {
            if (native == null)
            {
                Log($"{label} not loaded");
                return;
            }

            Canvas canvas = mediaSlot.GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            // Card bounds: header row (Icon -> IconFrame -> row) through
            // CTA row (Text -> CtaBtn -> CtaRow)
            var headerRow = (RectTransform)icon.transform.parent.parent;
            var ctaRow = (RectTransform)cta.transform.parent.parent;

            var (mediaMin, mediaMax) = GetScreenRect(mediaSlot, cam);
            var (topMin, topMax) = GetScreenRect(headerRow, cam);
            var (bottomMin, bottomMax) = GetScreenRect(ctaRow, cam);

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

            // One rect covering the whole card = everything clickable
            var clickableRects = new[] { new RectInt(x, y, w, h) };

            native.Attach(x, y, w, h, mx, my, mw, mh, clickableRects);
            Log($"{label} attached at ({x},{y} {w}x{h}), whole card clickable");
        }

        (Vector2 min, Vector2 max) GetScreenRect(RectTransform rt, Camera cam)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
            return (min, max);
        }

        void ShowAdDisplay(VungleNative native, TMP_Text titleL, TMP_Text ratingL,
            TMP_Text bodyL, TMP_Text ctaL, RawImage iconImg)
        {
            titleL.text = string.IsNullOrEmpty(native.AdTitle) ? "--" : native.AdTitle;
            ratingL.text = "Rating: " + (native.AdStarRating > 0 ? native.AdStarRating.ToString("F1") : "0.0");
            bodyL.text = string.IsNullOrEmpty(native.AdBody) ? "" : native.AdBody;
            ctaL.text = string.IsNullOrEmpty(native.AdCallToAction) ? "" : native.AdCallToAction;
            if (!string.IsNullOrEmpty(native.AdIconUrl))
                StartCoroutine(LoadIcon(native.AdIconUrl, iconImg));
        }

        IEnumerator LoadIcon(string url, RawImage target)
        {
            yield return UIHelper.LoadImageFromPath(url, target);
            if (target.texture != null)
            {
                var fitter = target.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                    fitter.aspectRatio = (float)target.texture.width / target.texture.height;
            }
        }

        void ClearAdDisplay(TMP_Text titleL, TMP_Text ratingL, TMP_Text bodyL,
            TMP_Text ctaL, RawImage iconImg)
        {
            titleL.text = "--";
            ratingL.text = "Rating: --";
            bodyL.text = "";
            ctaL.text = "";
            iconImg.texture = null;
            iconImg.color = new Color(0.2f, 0.2f, 0.25f);
            var fitter = iconImg.GetComponent<AspectRatioFitter>();
            if (fitter != null) fitter.aspectRatio = 1f;
        }

        void OnAttachBoth()
        {
            Log("Attach Both tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (topNative != null && topNative.CanPlay())
            {
                AttachToSlot(topNative, topMediaSlot, topIcon, topCta, "Top native");
                ShowAdDisplay(topNative, topTitle, topRating, topBody, topCta, topIcon);
            }
            if (bottomNative != null && bottomNative.CanPlay())
            {
                AttachToSlot(bottomNative, bottomMediaSlot, bottomIcon, bottomCta, "Bottom native");
                ShowAdDisplay(bottomNative, bottomTitle, bottomRating, bottomBody, bottomCta, bottomIcon);
            }
    #else
            Log("Native not supported in editor");
    #endif
        }

        void OnDetachBoth()
        {
            Log("Detach Both tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            topNative?.Detach();
            bottomNative?.Detach();
            ClearAdDisplay(topTitle, topRating, topBody, topCta, topIcon);
            ClearAdDisplay(bottomTitle, bottomRating, bottomBody, bottomCta, bottomIcon);
            Log("Both natives detached");
    #else
            Log("Native not supported in editor");
    #endif
        }

        void OnLoadBoth()
        {
            Log("Load Both tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            string placementId = VungleConstants.NativePlacementId;

            // Destroy previous ads so their native views are unregistered
            // and released before new ones take over
            topNative?.Destroy();
            bottomNative?.Destroy();

            topNative = new VungleNative(placementId);
            topNative.onLoadSuccess += () => Log("Top native loaded");
            topNative.onLoadFailed += (err) => Log("Top native load failed: " + err);
            topNative.onDidPresent += () => Log("Top native presented");
            topNative.onPresentFailed += (err) => Log("Top native present failed: " + err);
            topNative.onDidClose += () => { Log("Top native closed"); topNative = null; };
            topNative.onImpression += () => Log("Top native impression");
            topNative.onClick += () => Log("Top native click");
            topNative.onAdDataReceived += (title, body, cta, rating, iconUrl) =>
            {
                Log($"Top native data: title={title}, cta={cta}, rating={rating}");
            };
            topNative.Load();
            Log("Top native loading...");

            bottomNative = new VungleNative(placementId);
            bottomNative.onLoadSuccess += () => Log("Bottom native loaded");
            bottomNative.onLoadFailed += (err) => Log("Bottom native load failed: " + err);
            bottomNative.onDidPresent += () => Log("Bottom native presented");
            bottomNative.onPresentFailed += (err) => Log("Bottom native present failed: " + err);
            bottomNative.onDidClose += () => { Log("Bottom native closed"); bottomNative = null; };
            bottomNative.onImpression += () => Log("Bottom native impression");
            bottomNative.onClick += () => Log("Bottom native click");
            bottomNative.onAdDataReceived += (title, body, cta, rating, iconUrl) =>
            {
                Log($"Bottom native data: title={title}, cta={cta}, rating={rating}");
            };
            bottomNative.Load();
            Log("Bottom native loading...");
    #else
            Log("Native not supported in editor");
    #endif
        }
    }
}
