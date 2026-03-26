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

        TMP_InputField appIdInput;
        TMP_InputField placementInput;
        TextMeshProUGUI statusLabel;
        TextMeshProUGUI logText;
        ScrollRect logScroll;

        Button loadBtn;
        Button playBtn;
        Button shutdownBtn;

        void Start()
        {
            var content = LiftoffUIHelper.SetupScene();

            LiftoffUIHelper.CreateTitle("Liftoff Windows SDK", content);

            statusLabel = LiftoffUIHelper.CreateLabel("Status: Not Initialized", content,
                fontSize: 20, style: FontStyles.Italic,
                alignment: TextAlignmentOptions.Center, height: 30);
            statusLabel.color = new Color(0.55f, 0.55f, 0.6f);

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // --- Configuration ---
            LiftoffUIHelper.CreateLabel("Configuration", content, 26, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);

            LiftoffUIHelper.CreateLabel("App ID", content, 20, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 28);
            appIdInput = LiftoffUIHelper.CreateInputField("Enter App ID", content,
                defaultValue: "YOUR_APP_ID");

            LiftoffUIHelper.CreateLabel("Placement", content, 20, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, 28);
            placementInput = LiftoffUIHelper.CreateInputField("Enter Placement ID", content,
                defaultValue: "YOUR_PLACEMENT");

            LiftoffUIHelper.CreateSpacer(content, 6);

            // --- SDK Controls ---
            var initRow = LiftoffUIHelper.CreateHorizontalGroup(content, 60);
            LiftoffUIHelper.CreateButton("Initialize", initRow, OnInitClicked);
            shutdownBtn = LiftoffUIHelper.CreateButton("Shutdown", initRow, OnShutdownClicked,
                color: new Color(0.27f, 0.27f, 0.32f));

            var adRow = LiftoffUIHelper.CreateHorizontalGroup(content, 60);
            loadBtn = LiftoffUIHelper.CreateButton("Load Ad", adRow, OnLoadClicked);
            playBtn = LiftoffUIHelper.CreateButton("Play Ad", adRow, OnPlayClicked,
                color: new Color(0.18f, 0.62f, 0.34f));

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // --- Privacy Settings ---
            LiftoffUIHelper.CreateLabel("Privacy Settings", content, 26, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);

            var coppaRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("COPPA On", coppaRow,
                () => { SetPrivacy("COPPA", () => LiftoffWindows.SetCoppaStatus(true)); },
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("COPPA Off", coppaRow,
                () => { SetPrivacy("COPPA Off", () => LiftoffWindows.SetCoppaStatus(false)); },
                color: new Color(0.27f, 0.27f, 0.32f));

            var ccpaRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("CCPA Opt-In", ccpaRow,
                () => { SetPrivacy("CCPA Opt-In", () => LiftoffWindows.SetCcpaStatus(true)); },
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("CCPA Opt-Out", ccpaRow,
                () => { SetPrivacy("CCPA Opt-Out", () => LiftoffWindows.SetCcpaStatus(false)); },
                color: new Color(0.27f, 0.27f, 0.32f));

            var gdprRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("GDPR Opt-In", gdprRow,
                () => { SetPrivacy("GDPR Opt-In", () => LiftoffWindows.SetGdprConsentStatus(true)); },
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("GDPR Opt-Out", gdprRow,
                () => { SetPrivacy("GDPR Opt-Out", () => LiftoffWindows.SetGdprConsentStatus(false)); },
                color: new Color(0.27f, 0.27f, 0.32f));

            var ashwidRow = LiftoffUIHelper.CreateHorizontalGroup(content, 50);
            LiftoffUIHelper.CreateButton("Disable ASHWID", ashwidRow,
                () => { SetPrivacy("Disable ASHWID", () => LiftoffWindows.SetDisableAshwidTracking(true)); },
                color: new Color(0.27f, 0.27f, 0.32f));
            LiftoffUIHelper.CreateButton("Enable ASHWID", ashwidRow,
                () => { SetPrivacy("Enable ASHWID", () => LiftoffWindows.SetDisableAshwidTracking(false)); },
                color: new Color(0.27f, 0.27f, 0.32f));

            LiftoffUIHelper.CreateSpacer(content, 6);
            LiftoffUIHelper.CreateSeparator(content);
            LiftoffUIHelper.CreateSpacer(content, 6);

            // --- Log ---
            LiftoffUIHelper.CreateLabel("Event Log", content, 26, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);

            var (lt, sr) = LiftoffUIHelper.CreateLogArea(content, 500);
            logText = lt;
            logScroll = sr;

            LiftoffUIHelper.CreateButton("Clear Log", content,
                () => { logText.text = string.Empty; },
                50, new Color(0.38f, 0.38f, 0.43f));

            LiftoffUIHelper.CreateSpacer(content, 30);

            UpdateStatus();
        }

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

        void OnSdkInitialized()
        {
            LogUI("[Liftoff] Initialized.");
            UpdateStatus();
        }

        void OnSdkInitFailed(int c, string m)
        {
            LogUI($"[Liftoff] Init failed {c}: {m}");
            UpdateStatus();
        }

        void OnSdkAdLoaded(string p) => LogUI($"[Liftoff] Ad loaded: {p}");
        void OnSdkAdLoadFailed(string p, int c, string m) => LogUI($"[Liftoff] Load failed {p}: {c} {m}");
        void OnSdkAdStart(string p, string eid) => LogUI($"[Liftoff] Ad started {p} eid={eid}");
        void OnSdkAdEnd(string p) => LogUI($"[Liftoff] Ad ended: {p}");
        void OnSdkAdPlayFailed(string p, int c, string m) => LogUI($"[Liftoff] Play failed {p}: {c} {m}");
        void OnSdkAdRewarded(string p) => LogUI($"[Liftoff] Rewarded: {p}");
        void OnSdkAdClick(string p) => LogUI($"[Liftoff] Click: {p}");
        void OnSdkDiagnostic(int lvl, string sender, string msg) => LogUI($"[Diag:{lvl}] {sender}: {msg}");

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

        void LogUI(string msg)
        {
            Debug.Log(msg);
            if (logText != null)
            {
                logText.text += msg + "\n";
                Canvas.ForceUpdateCanvases();
                if (logScroll != null)
                    logScroll.normalizedPosition = Vector2.zero;
            }
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

        void OnInitClicked()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            string id = appIdInput != null ? appIdInput.text : "YOUR_APP_ID";
            IntPtr hwnd = IntPtr.Zero;
            try { hwnd = GetActiveWindow(); } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle; } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = GetForegroundWindow(); } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = FindWindow("UnityWndClass", null); } catch {}

            LogUI($"[Liftoff] Initializing with HWND=0x{hwnd.ToInt64():X}");
            LiftoffWindows.Initialize(id, hwnd);
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
            string p = placementInput != null ? placementInput.text : "YOUR_PLACEMENT";
            LiftoffWindows.LoadAd(p);
            LogUI($"[Liftoff] LoadAd('{p}') called.");
        }

        void OnPlayClicked()
        {
            string p = placementInput != null ? placementInput.text : "YOUR_PLACEMENT";
            LiftoffWindows.PlayAd(p);
            LogUI($"[Liftoff] PlayAd('{p}') called.");
        }

        void OnApplicationQuit()
        {
            LiftoffWindows.Shutdown();
        }
    }
}
