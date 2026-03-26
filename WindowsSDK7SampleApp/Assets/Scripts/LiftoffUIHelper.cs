using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Liftoff.Windows
{
    public static class LiftoffUIHelper
    {
        static readonly Color BgColor = new Color(0.12f, 0.12f, 0.14f);
        static readonly Color ButtonColor = new Color(0.22f, 0.47f, 0.88f);
        static readonly Color SecondaryBtnColor = new Color(0.27f, 0.27f, 0.32f);
        static readonly Color InputBgColor = new Color(0.18f, 0.18f, 0.22f);
        static readonly Color TextColor = new Color(0.93f, 0.93f, 0.93f);
        static readonly Color DimTextColor = new Color(0.55f, 0.55f, 0.6f);

        public static Transform SetupScene()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = BgColor;
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(canvasGo.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(30, 30);
            scrollRt.offsetMax = new Vector2(-30, -30);

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 40;
            scrollGo.AddComponent<Image>().color = Color.clear;
            scrollGo.AddComponent<Mask>().showMaskGraphic = false;

            var viewport = scrollGo.GetComponent<RectTransform>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = new Vector2(0, 0);
            contentRt.offsetMax = new Vector2(0, 0);

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewport;

            return contentGo.transform;
        }

        public static TextMeshProUGUI CreateTitle(string text, Transform parent)
        {
            var go = new GameObject("Title");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 60;
            le.preferredHeight = 60;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = TextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        public static TextMeshProUGUI CreateLabel(string text, Transform parent,
            float fontSize = 24, FontStyles style = FontStyles.Normal,
            TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft,
            float height = 35)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = TextColor;
            tmp.alignment = alignment;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        public static Button CreateButton(string label, Transform parent, Action onClick,
            float height = 60, Color? color = null)
        {
            var go = new GameObject("Button_" + label);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var img = go.AddComponent<Image>();
            img.color = color ?? ButtonColor;
            img.type = Image.Type.Sliced;

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
            cb.pressedColor = new Color(0.65f, 0.65f, 0.65f);
            cb.disabledColor = new Color(0.4f, 0.4f, 0.4f);
            btn.colors = cb;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 0);
            textRt.offsetMax = new Vector2(-10, 0);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            btn.onClick.AddListener(() => onClick?.Invoke());
            return btn;
        }

        public static TMP_InputField CreateInputField(string placeholder, Transform parent,
            float height = 55, string defaultValue = "")
        {
            var go = new GameObject("InputField");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var img = go.AddComponent<Image>();
            img.color = InputBgColor;

            var inputField = go.AddComponent<TMP_InputField>();

            var textArea = new GameObject("TextArea");
            textArea.transform.SetParent(go.transform, false);
            var textAreaRt = textArea.GetComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(15, 5);
            textAreaRt.offsetMax = new Vector2(-15, -5);
            textArea.AddComponent<RectMask2D>();

            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(textArea.transform, false);
            var phRt = phGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;
            var phTmp = phGo.AddComponent<TextMeshProUGUI>();
            phTmp.text = placeholder;
            phTmp.fontSize = 22;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = DimTextColor;
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(textArea.transform, false);
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            var txtTmp = txtGo.AddComponent<TextMeshProUGUI>();
            txtTmp.fontSize = 22;
            txtTmp.color = TextColor;
            txtTmp.alignment = TextAlignmentOptions.MidlineLeft;

            inputField.textViewport = textAreaRt;
            inputField.textComponent = txtTmp;
            inputField.placeholder = phTmp;
            inputField.text = defaultValue;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.fontAsset = txtTmp.font;

            return inputField;
        }

        public static void CreateSpacer(Transform parent, float height = 8)
        {
            var go = new GameObject("Spacer");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
        }

        public static void CreateSeparator(Transform parent)
        {
            var go = new GameObject("Separator");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 2;
            le.preferredHeight = 2;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.35f);
        }

        public static (TextMeshProUGUI logText, ScrollRect scrollRect) CreateLogArea(
            Transform parent, float height = 400)
        {
            var container = new GameObject("LogContainer");
            container.transform.SetParent(parent, false);
            var containerLe = container.AddComponent<LayoutElement>();
            containerLe.minHeight = height;
            containerLe.preferredHeight = height;

            var containerImg = container.AddComponent<Image>();
            containerImg.color = new Color(0.08f, 0.08f, 0.1f);

            var sr = container.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30;

            var mask = container.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var contentGo = new GameObject("LogContent");
            contentGo.transform.SetParent(container.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textGo = new GameObject("LogText");
            textGo.transform.SetParent(contentGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 1);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.pivot = new Vector2(0.5f, 1);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.color = new Color(0.7f, 0.9f, 0.7f);
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.margin = new Vector4(12, 8, 12, 8);
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = true;

            var txtCsf = textGo.AddComponent<ContentSizeFitter>();
            txtCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = contentRt;
            sr.viewport = container.GetComponent<RectTransform>();

            return (tmp, sr);
        }

        public static RectTransform CreateHorizontalGroup(Transform parent, float height = 55,
            float spacing = 10)
        {
            var outer = new GameObject("HGroup");
            outer.transform.SetParent(parent, false);
            var le = outer.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var inner = new GameObject("HGroupInner");
            inner.transform.SetParent(outer.transform, false);
            var innerRt = inner.GetComponent<RectTransform>();
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = Vector2.zero;
            innerRt.offsetMax = Vector2.zero;

            var hlg = inner.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            return innerRt;
        }
    }
}
