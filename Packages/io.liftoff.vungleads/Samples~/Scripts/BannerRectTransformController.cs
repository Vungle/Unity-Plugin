using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using VungleAds;

namespace VungleAds.Samples
{
    public class BannerRectTransformController : MonoBehaviour
    {
        TMP_Text logText;
        ScrollRect logScroll;
        bool logDirty;
        RectTransform adSlot;

    #if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern float VungleGetNativeScreenScale();
    #endif

    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        VungleBannerView bannerAd;
    #endif

        void Start()
        {
            var content = UIHelper.SetupScene();

            UIHelper.CreateBackButton(content, OnBack);
            UIHelper.CreateTitle("Banner (RectTransform)", content);
            UIHelper.CreateSpacer(content, 8);

            UIHelper.CreateButton("Load", content, OnLoad);
            UIHelper.CreateButton("Attach to Slot", content, OnAttach);
            UIHelper.CreateButton("Destroy", content, OnDestroyBanner);

            UIHelper.CreateSpacer(content, 12);

            adSlot = CreateAdSlot(content);

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

        RectTransform CreateAdSlot(Transform parent)
        {
            // Compute canvas units so the native view renders at exactly 300×250 dp/pt.
            // iOS bridge does: frame = screenPixels / [UIScreen mainScreen].scale
            // Android bridge uses raw screen pixels with DisplayMetrics.density.
            // Both require the exact native scale, not an approximation from Screen.dpi.
            //
            // canvas.scaleFactor is NOT used here because CanvasScaler.Update() hasn't run yet
            // in Start() — it fires with default settings (scaleFactor=1) on AddComponent, then
            // corrects itself in the first Update(). Recompute using the same formula directly.
            float logScaleX = Mathf.Log(Screen.width / 1080f, 2f);
            float logScaleY = Mathf.Log(Screen.height / 1920f, 2f);
            float canvasScale = Mathf.Pow(2f, Mathf.Lerp(logScaleX, logScaleY, 0.5f));

            float nativeScale;
    #if UNITY_IOS && !UNITY_EDITOR
            nativeScale = VungleGetNativeScreenScale();
    #elif UNITY_ANDROID && !UNITY_EDITOR
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var resources = activity.Call<AndroidJavaObject>("getResources"))
            using (var metrics = resources.Call<AndroidJavaObject>("getDisplayMetrics"))
                nativeScale = metrics.Get<float>("density");
    #else
            nativeScale = Screen.dpi > 0 ? Screen.dpi / 160f : 1f;
    #endif

            float slotW = 300f * nativeScale / canvasScale;
            float slotH = 250f * nativeScale / canvasScale;

            // Full-width container fixes height
            var container = new GameObject("AdSlotContainer");
            container.transform.SetParent(parent, false);
            var containerLE = container.AddComponent<LayoutElement>();
            containerLE.minHeight = slotH;
            containerLE.preferredHeight = slotH;

            // Inner slot: MREC dimensions centered in container
            var slotObj = new GameObject("AdSlot", typeof(RectTransform));
            slotObj.transform.SetParent(container.transform, false);
            var slotRT = (RectTransform)slotObj.transform;
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.anchoredPosition = Vector2.zero;
            slotRT.sizeDelta = new Vector2(slotW, slotH);

            slotObj.AddComponent<Image>().color = new Color(0.18f, 0.32f, 0.22f);

            var outline = slotObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.7f, 0.4f, 0.8f);
            outline.effectDistance = new Vector2(3, 3);

            var labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(slotObj.transform, false);
            var labelRT = (RectTransform)labelObj.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "Ad Slot (MREC 300×250)";
            label.fontSize = 28;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(1f, 1f, 1f, 0.35f);
            label.alignment = TextAlignmentOptions.Center;

            return slotRT;
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

            bannerAd = new VungleBannerView(VungleConstants.MrecPlacementId, VungleBannerSize.Mrec);
            bannerAd.onLoadSuccess   = ()  => Log("Banner loaded — tap Attach to Slot");
            bannerAd.onLoadFailed    = err => Log("Banner load failed: " + err);
            bannerAd.onWillPresent   = ()  => Log("Banner will present");
            bannerAd.onDidPresent    = ()  => Log("Banner presented");
            bannerAd.onPresentFailed = err => Log("Banner present failed: " + err);
            bannerAd.onImpression    = ()  => Log("Banner impression");
            bannerAd.onClick         = ()  => Log("Banner click");
            bannerAd.onWillLeaveApplication = () => Log("Leaving app via ad");
            bannerAd.onWillClose     = ()  => Log("Banner will close");
            bannerAd.onDidClose      = ()  => { Log("Banner closed"); bannerAd = null; };
            bannerAd.Load();
            Log("Banner loading (MREC)...");
    #else
            Log("Banner not supported in editor");
    #endif
        }

        void OnAttach()
        {
            Log("Attach tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (bannerAd == null)
            {
                Log("No banner loaded");
                return;
            }
            bannerAd.Attach(adSlot);
            Log("Banner attached to RectTransform slot");
    #else
            Log("Banner not supported in editor");
    #endif
        }

        void OnDestroyBanner()
        {
            Log("Destroy tapped");
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            bannerAd?.Destroy();
            bannerAd = null;
            Log("Banner destroyed");
    #else
            Log("Banner not supported in editor");
    #endif
        }
    }
}
