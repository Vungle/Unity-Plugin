using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace VungleAds.Samples
{
    public static class UIHelper
    {
        static bool fontFallbackInitialized;

        // Ad text is arbitrary publisher/advertiser content (CJK, Cyrillic,
        // Arabic, ...). The TMP Essential Resources font (LiberationSans SDF)
        // only covers Latin, so register dynamic fallback font assets built
        // from OS fonts — missing glyphs are then rasterized on demand.
        // (Color emoji are not covered by SDF text rendering.)
        static void EnsureFontFallback()
        {
            if (fontFallbackInitialized) return;
            fontFallbackInitialized = true;

            string[] installed = Font.GetOSInstalledFontNames();
            Debug.Log("[VungleSample] OS fonts: " + string.Join(", ", installed));

            // Substring match by priority: device font naming varies — e.g.
            // Android's NotoSansCJK-Regular.ttc exposes per-locale faces like
            // "Noto Sans CJK KR" — so exact names are too fragile.
            string[] wanted =
            {
                "CJK",                                        // Android pan-CJK (KR/JP/SC/TC faces)
                "Noto Sans KR", "Noto Sans JP", "Noto Sans SC",
                "PingFang", "Hiragino", "Apple SD Gothic",    // iOS CJK
                "Noto Sans", "Roboto", "Helvetica",           // Latin & friends
            };
            var added = new System.Collections.Generic.List<string>();
            foreach (string pattern in wanted)
            {
                foreach (string name in installed)
                {
                    if (added.Count >= 6) break; // bound the fallback chain
                    if (name.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (added.Contains(name)) continue;
                    Font osFont = Font.CreateDynamicFontFromOSFont(name, 24);
                    if (osFont == null) continue;
                    TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(osFont);
                    if (asset == null) continue;
                    TMP_Settings.fallbackFontAssets.Add(asset);
                    added.Add(name);
                }
            }
            Debug.Log("[VungleSample] TMP fallback fonts added: "
                + (added.Count > 0 ? string.Join(", ", added) : "NONE"));
        }

        public static RectTransform SetupScene()
        {
            EnsureFontFallback();

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            var bg = AddChild("Background", canvasObj.transform);
            Stretch(bg);
            bg.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f);

            var safeArea = Screen.safeArea;
            var safeRT = AddChild("SafeArea", bg);
            safeRT.anchorMin = new Vector2(safeArea.x / Screen.width, safeArea.y / Screen.height);
            safeRT.anchorMax = new Vector2((safeArea.x + safeArea.width) / Screen.width,
                                            (safeArea.y + safeArea.height) / Screen.height);
            safeRT.offsetMin = Vector2.zero;
            safeRT.offsetMax = Vector2.zero;

            var content = AddChild("Content", safeRT);
            Stretch(content);
            content.offsetMin = new Vector2(24, 24);
            content.offsetMax = new Vector2(-24, -24);

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            return content;
        }

        public static TMP_Text CreateTitle(string text, Transform parent)
        {
            return CreateLabel(text, parent, 40, FontStyles.Bold, TextAlignmentOptions.Center, 70);
        }

        public static TMP_Text CreateLabel(string text, Transform parent, float fontSize = 26,
            FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            float height = 40)
        {
            var obj = new GameObject("Label");
            obj.transform.SetParent(parent, false);
            var le = obj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var t = obj.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = alignment;
            t.color = Color.white;
            return t;
        }

        public static Button CreateButton(string text, Transform parent, System.Action onClick,
            float height = 70, Color? color = null)
        {
            var btnColor = color ?? new Color(0.2f, 0.55f, 0.95f);

            var obj = new GameObject("Btn_" + text);
            obj.transform.SetParent(parent, false);
            var le = obj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var img = obj.AddComponent<Image>();
            img.color = btnColor;

            var btn = obj.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            btn.colors = cb;

            if (onClick != null)
                btn.onClick.AddListener(() => onClick());

            var textRT = AddChild("Text", obj.transform);
            Stretch(textRT);
            var label = textRT.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 28;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;

            return btn;
        }

        public static Button CreateBackButton(Transform parent, System.Action onClick)
        {
            return CreateButton("\u2190  Back", parent, onClick, 60, new Color(0.25f, 0.30f, 0.40f));
        }

        public static (TMP_Text logText, ScrollRect scrollRect) CreateLogArea(Transform parent)
        {
            var container = new GameObject("LogContainer");
            container.transform.SetParent(parent, false);
            var le = container.AddComponent<LayoutElement>();
            le.flexibleHeight = 1;
            le.minHeight = 120;

            var scrollRT = AddChild("Scroll", container.transform);
            Stretch(scrollRT);
            var scroll = scrollRT.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scrollRT.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f);

            var vpRT = AddChild("Viewport", scrollRT);
            Stretch(vpRT);
            vpRT.offsetMin = new Vector2(10, 5);
            vpRT.offsetMax = new Vector2(-10, -5);
            vpRT.gameObject.AddComponent<Image>().color = Color.clear;
            vpRT.gameObject.AddComponent<RectMask2D>();

            var contentRT = AddChild("Content", vpRT);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = Vector2.zero;
            var csf = contentRT.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var logText = contentRT.gameObject.AddComponent<TextMeshProUGUI>();
            logText.fontSize = 20;
            logText.color = new Color(0.75f, 0.85f, 0.75f);
            logText.textWrappingMode = TextWrappingModes.Normal;
            logText.overflowMode = TextOverflowModes.Overflow;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            return (logText, scroll);
        }

        public static void CreateSpacer(Transform parent, float height)
        {
            var obj = new GameObject("Spacer");
            obj.transform.SetParent(parent, false);
            var spacerLE = obj.AddComponent<LayoutElement>();
            spacerLE.minHeight = height;
            spacerLE.preferredHeight = height;
        }

        public static TMP_InputField CreateInputField(string placeholder, Transform parent,
            float height = 55)
        {
            var obj = new GameObject("Input_" + placeholder);
            obj.transform.SetParent(parent, false);
            var le = obj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleWidth = 1;

            obj.AddComponent<Image>().color = new Color(0.22f, 0.22f, 0.28f);

            var textAreaRT = AddChild("TextArea", obj.transform);
            Stretch(textAreaRT);
            textAreaRT.offsetMin = new Vector2(10, 0);
            textAreaRT.offsetMax = new Vector2(-10, 0);
            textAreaRT.gameObject.AddComponent<RectMask2D>();

            var textRT = AddChild("Text", textAreaRT);
            Stretch(textRT);
            var text = textRT.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.richText = false;

            var phRT = AddChild("Placeholder", textAreaRT);
            Stretch(phRT);
            var ph = phRT.gameObject.AddComponent<TextMeshProUGUI>();
            ph.fontSize = 24;
            ph.fontStyle = FontStyles.Italic;
            ph.text = placeholder;
            ph.color = new Color(0.5f, 0.5f, 0.5f);
            ph.alignment = TextAlignmentOptions.MidlineLeft;

            var input = obj.AddComponent<TMP_InputField>();
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.textComponent = text;
            input.placeholder = ph;
            input.textViewport = textAreaRT;

            return input;
        }

        public static RectTransform CreateHorizontalGroup(Transform parent, float height = 55,
            float spacing = 10)
        {
            var outer = new GameObject("HGroup");
            outer.transform.SetParent(parent, false);
            var le = outer.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var inner = AddChild("HGroupInner", outer.transform);
            Stretch(inner);

            var hlg = inner.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            return inner;
        }

        public static RawImage CreateImage(Transform parent, float size = 150)
        {
            var obj = new GameObject("IconImage");
            obj.transform.SetParent(parent, false);
            var le = obj.AddComponent<LayoutElement>();
            le.minHeight = size;
            le.preferredHeight = size;
            le.minWidth = size;
            le.preferredWidth = size;

            var img = obj.AddComponent<RawImage>();
            img.color = new Color(0.2f, 0.2f, 0.25f); // placeholder bg
            return img;
        }

        public static IEnumerator LoadImageFromPath(string filePath, RawImage target)
        {
            if (string.IsNullOrEmpty(filePath) || target == null) yield break;

            string url = filePath;
            if (!url.StartsWith("file://") && !url.StartsWith("http"))
                url = "file://" + url;

            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    target.texture = DownloadHandlerTexture.GetContent(request);
                    target.color = Color.white;
                }
            }
        }

        static RectTransform AddChild(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return (RectTransform)obj.transform;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
