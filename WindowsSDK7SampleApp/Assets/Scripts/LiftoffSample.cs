using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Liftoff.Windows
{
    public class LiftoffSample : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
#endif

        // Persisted state across page navigations
        string logContent = "";
        string appId = "YOUR_APP_ID";
        string placement = "YOUR_PLACEMENT";

        // Current page UI references (rebuilt on each navigation)
        TextMeshProUGUI logText;
        ScrollRect logScroll;
        TextMeshProUGUI statusLabel;

        void Start() => ShowLaunchPage();

        void OnEnable()
        {
            LiftoffWindows.OnInitialized += OnSdkInitialized;
            LiftoffWindows.OnInitializationFailed += OnSdkInitFailed;
            LiftoffWindows.OnAdLoaded += OnSdkAdLoaded;
            LiftoffWindows.OnAdLoadFailed += OnSdkAdLoadFailed;
            LiftoffWindows.OnAdStart += OnSdkAdStart;
            LiftoffWindows.OnAdEnd += OnSdkAdEnd;
            LiftoffWindows.OnAdPlayFailed += OnSdkAdPlayFailed;
            LiftoffWindows.OnAdRewarded += OnSdkAdRewarded;
            LiftoffWindows.OnAdClick += OnSdkAdClick;
            LiftoffWindows.OnDiagnostic += OnSdkDiagnostic;
        }

        void OnDisable()
        {
            LiftoffWindows.OnInitialized -= OnSdkInitialized;
            LiftoffWindows.OnInitializationFailed -= OnSdkInitFailed;
            LiftoffWindows.OnAdLoaded -= OnSdkAdLoaded;
            LiftoffWindows.OnAdLoadFailed -= OnSdkAdLoadFailed;
            LiftoffWindows.OnAdStart -= OnSdkAdStart;
            LiftoffWindows.OnAdEnd -= OnSdkAdEnd;
            LiftoffWindows.OnAdPlayFailed -= OnSdkAdPlayFailed;
            LiftoffWindows.OnAdRewarded -= OnSdkAdRewarded;
            LiftoffWindows.OnAdClick -= OnSdkAdClick;
            LiftoffWindows.OnDiagnostic -= OnSdkDiagnostic;
        }

        // ---- SDK Event Handlers ----
        void OnSdkInitialized() { LogUI("[Liftoff] Initialized."); UpdateStatus(); }
        void OnSdkInitFailed(int c, string m) { LogUI($"[Liftoff] Init failed {c}: {m}"); UpdateStatus(); }
        void OnSdkAdLoaded(string p) => LogUI($"[Liftoff] Ad loaded: {p}");
        void OnSdkAdLoadFailed(string p, int c, string m) => LogUI($"[Liftoff] Load failed {p}: {c} {m}");
        void OnSdkAdStart(string p, string eid) => LogUI($"[Liftoff] Ad started {p} eid={eid}");
        void OnSdkAdEnd(string p) => LogUI($"[Liftoff] Ad ended: {p}");
        void OnSdkAdPlayFailed(string p, int c, string m) => LogUI($"[Liftoff] Play failed {p}: {c} {m}");
        void OnSdkAdRewarded(string p) => LogUI($"[Liftoff] Rewarded: {p}");
        void OnSdkAdClick(string p) => LogUI($"[Liftoff] Click: {p}");
        void OnSdkDiagnostic(int lvl, string sender, string msg) => LogUI($"[Diag:{lvl}] {sender}: {msg}");

        // ---- Shared Helpers ----
        void LogUI(string msg)
        {
            Debug.Log(msg);
            logContent += msg + "\n";
            // Cap log to prevent unbounded string growth and layout cost.
            if (logContent.Length > 10000)
                logContent = logContent.Substring(logContent.Length - 8000);
            if (logText != null)
            {
                logText.text = logContent;
                if (logScroll != null) logScroll.normalizedPosition = Vector2.zero;
            }
        }

        void UpdateStatus()
        {
            if (statusLabel == null) return;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            bool init = LiftoffWindows.IsInitialized;
            bool wv2 = LiftoffWindows.IsWebView2Available();
            statusLabel.text = $"SDK: {(init ? "Initialized" : "Not Initialized")}  |  WebView2: {(wv2 ? "Available" : "Not Found")}";
            statusLabel.color = init
                ? new Color(0.4f, 0.85f, 0.5f)
                : new Color(0.55f, 0.55f, 0.6f);
#else
            statusLabel.text = "Platform: Not Windows";
#endif
        }

        void SaveAppId(TMP_InputField field) { if (field != null) appId = field.text; }
        void SavePlacement(TMP_InputField field) { if (field != null) placement = field.text; }

        void BuildLogArea(Transform content, float height = 400)
        {
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            LiftoffUIHelper.CreateLabel("Event Log", content, 26, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);
            var (lt, sr) = LiftoffUIHelper.CreateLogArea(content, height);
            logText = lt;
            logScroll = sr;
            logText.text = logContent;

            LiftoffUIHelper.CreateButton("Clear Log", content,
                () => { logContent = ""; logText.text = ""; },
                50, new Color(0.38f, 0.38f, 0.43f));

            LiftoffUIHelper.CreateSpacer(content, 30);
        }

        // ============================================
        // LAUNCH PAGE (Hub)
        // ============================================
        void ShowLaunchPage()
        {
            var content = LiftoffUIHelper.SetupScene();
            LiftoffUIHelper.CreateTitle("Liftoff Windows SDK", content);

            statusLabel = LiftoffUIHelper.CreateLabel("Status: Not Initialized", content,
                fontSize: 20, style: FontStyles.Italic,
                alignment: TextAlignmentOptions.Center, height: 30);
            statusLabel.color = new Color(0.55f, 0.55f, 0.6f);
            UpdateStatus();

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // Configuration
            LiftoffUIHelper.CreateLabel("Configuration", content, 26, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);

            LiftoffUIHelper.CreateLabel("App ID", content, 20, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 28);
            var appIdInput = LiftoffUIHelper.CreateInputField("Enter App ID", content,
                defaultValue: appId);

            LiftoffUIHelper.CreateSpacer(content, 6);

            // SDK Controls
            var initRow = LiftoffUIHelper.CreateHorizontalGroup(content, 60);
            LiftoffUIHelper.CreateButton("Initialize", initRow, () =>
            {
                SaveAppId(appIdInput);
                OnInitClicked();
            });
            LiftoffUIHelper.CreateButton("Shutdown", initRow, OnShutdownClicked,
                color: new Color(0.27f, 0.27f, 0.32f));

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // Navigation
            LiftoffUIHelper.CreateLabel("Features", content, 26, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);

            LiftoffUIHelper.CreateButton("Ads", content, () =>
            {
                SaveAppId(appIdInput);
                ShowAdsPage();
            });
            LiftoffUIHelper.CreateButton("Privacy", content, () =>
            {
                SaveAppId(appIdInput);
                ShowPrivacyPage();
            });
            LiftoffUIHelper.CreateButton("Bidding", content, () =>
            {
                SaveAppId(appIdInput);
                ShowBiddingPage();
            });

            LiftoffUIHelper.CreateSpacer(content, 6);

            // Log
            BuildLogArea(content, 350);
        }

        // ============================================
        // ADS PAGE
        // ============================================
        void ShowAdsPage()
        {
            var content = LiftoffUIHelper.SetupScene();

            LiftoffUIHelper.CreateButton("< Back", content, ShowLaunchPage,
                50, new Color(0.38f, 0.38f, 0.43f));
            LiftoffUIHelper.CreateTitle("Ads", content);
            LiftoffUIHelper.CreateSpacer(content, 8);

            LiftoffUIHelper.CreateLabel("Placement", content, 20, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 28);
            var placementInput = LiftoffUIHelper.CreateInputField("Enter Placement ID", content,
                defaultValue: placement);

            LiftoffUIHelper.CreateSpacer(content, 6);

            var adRow = LiftoffUIHelper.CreateHorizontalGroup(content, 60);
            LiftoffUIHelper.CreateButton("Load Ad", adRow, () =>
            {
                SavePlacement(placementInput);
                OnLoadClicked();
            });
            LiftoffUIHelper.CreateButton("Play Ad", adRow, () =>
            {
                SavePlacement(placementInput);
                OnPlayClicked();
            }, color: new Color(0.18f, 0.62f, 0.34f));

            LiftoffUIHelper.CreateSpacer(content, 12);

            BuildLogArea(content);
        }

        // ============================================
        // PRIVACY PAGE
        // ============================================
        void ShowPrivacyPage()
        {
            var content = LiftoffUIHelper.SetupScene();

            LiftoffUIHelper.CreateButton("< Back", content, ShowLaunchPage,
                50, new Color(0.38f, 0.38f, 0.43f));
            LiftoffUIHelper.CreateTitle("Privacy", content);
            LiftoffUIHelper.CreateSpacer(content, 8);

            var coppaRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("COPPA On", coppaRow,
                () => SetPrivacy("COPPA", () => LiftoffWindows.SetCoppaStatus(true)),
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("COPPA Off", coppaRow,
                () => SetPrivacy("COPPA Off", () => LiftoffWindows.SetCoppaStatus(false)),
                color: new Color(0.27f, 0.27f, 0.32f));

            var ccpaRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("CCPA Opt-In", ccpaRow,
                () => SetPrivacy("CCPA Opt-In", () => LiftoffWindows.SetCcpaStatus(true)),
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("CCPA Opt-Out", ccpaRow,
                () => SetPrivacy("CCPA Opt-Out", () => LiftoffWindows.SetCcpaStatus(false)),
                color: new Color(0.27f, 0.27f, 0.32f));

            var gdprRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("GDPR Opt-In", gdprRow,
                () => SetPrivacy("GDPR Opt-In", () => LiftoffWindows.SetGdprConsentStatus(true)),
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("GDPR Opt-Out", gdprRow,
                () => SetPrivacy("GDPR Opt-Out", () => LiftoffWindows.SetGdprConsentStatus(false)),
                color: new Color(0.27f, 0.27f, 0.32f));

            var ashwidRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("Disable ASHWID", ashwidRow,
                () => SetPrivacy("Disable ASHWID", () => LiftoffWindows.SetDisableAshwidTracking(true)),
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("Enable ASHWID", ashwidRow,
                () => SetPrivacy("Enable ASHWID", () => LiftoffWindows.SetDisableAshwidTracking(false)),
                color: new Color(0.27f, 0.27f, 0.32f));

            LiftoffUIHelper.CreateSpacer(content, 12);

            BuildLogArea(content);
        }

        // ============================================
        // BIDDING PAGE
        // ============================================
        void ShowBiddingPage()
        {
            var content = LiftoffUIHelper.SetupScene();
            TextMeshProUGUI tokenDisplay = null;
            TMP_InputField markupInput = null;

            LiftoffUIHelper.CreateButton("< Back", content, ShowLaunchPage,
                50, new Color(0.38f, 0.38f, 0.43f));
            LiftoffUIHelper.CreateTitle("Bidding", content);
            LiftoffUIHelper.CreateSpacer(content, 8);

            // Placement
            LiftoffUIHelper.CreateLabel("Placement", content, 20, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 28);
            var placementInput = LiftoffUIHelper.CreateInputField("Enter Placement ID", content,
                defaultValue: placement);

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // Step 1: Get Super Token
            LiftoffUIHelper.CreateLabel("1. Get Super Token", content, 22, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 30);
            LiftoffUIHelper.CreateButton("Get Super Token", content, () =>
            {
                SavePlacement(placementInput);
                OnGetSuperTokenClicked(tokenDisplay);
            });

            var (td, _) = LiftoffUIHelper.CreateLogArea(content, 150);
            tokenDisplay = td;
            tokenDisplay.text = "(No token retrieved yet)";
            tokenDisplay.color = new Color(0.85f, 0.85f, 0.95f);

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // Step 2: Enter markup from auction response
            LiftoffUIHelper.CreateLabel("2. Bidding Markup (from auction)", content, 22, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 30);
            markupInput = LiftoffUIHelper.CreateInputField("Paste header bidding markup", content);

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // Step 3: Load & Play with markup
            LiftoffUIHelper.CreateLabel("3. Load & Play", content, 22, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, 30);

            LiftoffUIHelper.CreateButton("Load Bidding Ad", content, () =>
            {
                SavePlacement(placementInput);
                string markup = markupInput != null ? markupInput.text : "";
                OnLoadBiddingClicked(markup);
            });

            LiftoffUIHelper.CreateSpacer(content, 4);

            LiftoffUIHelper.CreateButton("Play Bidding Ad", content, () =>
            {
                SavePlacement(placementInput);
                string markup = markupInput != null ? markupInput.text : "";
                OnPlayBiddingClicked(markup);
            }, color: new Color(0.18f, 0.62f, 0.34f));

            LiftoffUIHelper.CreateSpacer(content, 6);

            BuildLogArea(content, 250);
        }

        // ---- Action Handlers ----
        void OnInitClicked()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            IntPtr hwnd = IntPtr.Zero;
            try { hwnd = GetActiveWindow(); } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle; } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = GetForegroundWindow(); } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = FindWindow("UnityWndClass", null); } catch {}

            LogUI($"[Liftoff] Initializing with HWND=0x{hwnd.ToInt64():X}");
            LiftoffWindows.Initialize(appId, hwnd);
            LogUI($"[Liftoff] Initialize called. WebView2: {LiftoffWindows.IsWebView2Available()}");
            UpdateStatus();
#else
            LogUI("[Liftoff] Not a Windows platform.");
#endif
        }

        void OnShutdownClicked()
        {
            LiftoffWindows.Shutdown();
            LogUI("[Liftoff] Shutdown called.");
            UpdateStatus();
        }

        void OnLoadClicked()
        {
            LiftoffWindows.LoadAd(placement);
            LogUI($"[Liftoff] LoadAd('{placement}') called.");
        }

        void OnPlayClicked()
        {
            LiftoffWindows.PlayAd(placement);
            LogUI($"[Liftoff] PlayAd('{placement}') called.");
        }

        void OnGetSuperTokenClicked(TextMeshProUGUI tokenDisplay)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            LogUI($"[Liftoff] GetSuperToken('{placement}') called.");
            string token = LiftoffWindows.GetSuperToken(placement);
            if (tokenDisplay != null)
            {
                tokenDisplay.text = string.IsNullOrEmpty(token)
                    ? "(No token returned - SDK may not be initialized or placement is invalid)"
                    : token;
            }
            LogUI(string.IsNullOrEmpty(token)
                ? "[Liftoff] Super token: null/empty"
                : $"[Liftoff] Super token retrieved ({token.Length} chars)");
#else
            LogUI("[Liftoff] GetSuperToken: not supported on this platform.");
            if (tokenDisplay != null)
                tokenDisplay.text = "(Not supported on this platform)";
#endif
        }

        void OnLoadBiddingClicked(string markup)
        {
            if (string.IsNullOrWhiteSpace(markup))
            {
                LogUI("[Liftoff] Bidding markup is empty. Paste the auction response first.");
                return;
            }
            LiftoffWindows.LoadAd(placement, markup);
            LogUI($"[Liftoff] LoadAd('{placement}', markup[{markup.Length}]) called.");
        }

        void OnPlayBiddingClicked(string markup)
        {
            if (string.IsNullOrWhiteSpace(markup))
            {
                LogUI("[Liftoff] Bidding markup is empty. Paste the auction response first.");
                return;
            }
            LiftoffWindows.PlayAd(placement, markup);
            LogUI($"[Liftoff] PlayAd('{placement}', markup[{markup.Length}]) called.");
        }

        void SetPrivacy(string label, Action action)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            action();
            LogUI($"[Privacy] {label} set.");
#else
            LogUI("[Privacy] Not supported on this platform.");
#endif
        }

        void OnApplicationQuit()
        {
            LiftoffWindows.Shutdown();
        }
    }
}
