// LiftoffSample.cs (demo/driver)
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

namespace Liftoff.Windows
{
    public class LiftoffSample : MonoBehaviour
    {
        [Header("Liftoff App ID")]
        public string appId = "YOUR_APP_ID";
        [Header("Placement")]
        public string placement = "YOUR_PLACEMENT";
        public TMP_Text text;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
#endif

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

        void OnSdkInitialized() => LogUI("[Liftoff] Initialized (event).");
        void OnSdkInitFailed(int code, string msg) => LogUI($"[Liftoff] Init failed {code}: {msg}");
        void OnSdkAdLoaded(string p) => LogUI($"[Liftoff] Loaded: {p}");
        void OnSdkAdLoadFailed(string p, int code, string msg) => LogUI($"[Liftoff] Load fail {p}: {code} {msg}");
        void OnSdkAdStart(string p, string eid) => LogUI($"[Liftoff] Start {p} eid={eid}");
        void OnSdkAdEnd(string p) => LogUI($"[Liftoff] End {p}");
        void OnSdkAdPlayFailed(string p, int code, string msg) => LogUI($"[Liftoff] Play fail {p}: {code} {msg}");
        void OnSdkAdRewarded(string p) => LogUI($"[Liftoff] Rewarded {p}");
        void OnSdkAdClick(string p) => LogUI($"[Liftoff] Click {p}");
        void OnSdkDiagnostic(int lvl, string sender, string msg) => LogUI($"[{lvl}] {sender}: {msg}");

        void LogUI(string msg)
        {
            Debug.Log(msg);
            if (text != null) text.text = msg + "\n" + text.text;
        }

        public void OnInitClicked()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            IntPtr hwnd = IntPtr.Zero;
            try { hwnd = GetActiveWindow(); } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle; } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = GetForegroundWindow(); } catch {}
            if (hwnd == IntPtr.Zero) try { hwnd = FindWindow("UnityWndClass", null); } catch {}
            LogUI($"[Liftoff] Initialize with HWND=0x{hwnd.ToInt64():X} (0 means hidden host will be used).");

            LiftoffWindows.Initialize(appId, hwnd);
            LogUI($"[Liftoff] Initialize called. WebView2 available: {LiftoffWindows.IsWebView2Available()}");
#else
            LogUI("[Liftoff] Initialize: non-Windows platform.");
#endif
        }

        public void OnLoadClicked()
        {
            LiftoffWindows.LoadAd(placement);
            LogUI($"[Liftoff] LoadAd('{placement}') called");
        }

        public void OnPlayClicked()
        {
            LiftoffWindows.PlayAd(placement);
            LogUI($"[Liftoff] PlayAd('{placement}') called");
        }

        void OnApplicationQuit()
        {
            LiftoffWindows.Shutdown();
        }
    }
}
