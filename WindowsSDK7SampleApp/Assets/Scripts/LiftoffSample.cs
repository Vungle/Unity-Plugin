// LiftoffSample.cs (demo/driver)
using System;
using System.Collections;
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
            LiftoffWindows.OnInitialized += () =>
            {
                LogUI("[Liftoff] Initialized (event).");
            };
            LiftoffWindows.OnInitializationFailed += (c, m) => LogUI($"[Liftoff] Init failed {c}: {m}");
            LiftoffWindows.OnAdLoaded += p => { LogUI($"[Liftoff] Loaded: {p}"); };
            LiftoffWindows.OnAdLoadFailed += (p, c, m) => LogUI($"[Liftoff] Load fail {p}: {c} {m}");
            LiftoffWindows.OnAdStart += (p, eid) => LogUI($"[Liftoff] Start {p} eid={eid}");
            LiftoffWindows.OnAdEnd += p => LogUI($"[Liftoff] End {p}");
            LiftoffWindows.OnAdPlayFailed += (p, c, m) => LogUI($"[Liftoff] Play fail {p}: {c} {m}");
            LiftoffWindows.OnAdRewarded += p => LogUI($"[Liftoff] Rewarded {p}");
            LiftoffWindows.OnAdClick += p => LogUI($"[Liftoff] Click {p}");
            LiftoffWindows.OnDiagnostic += (lvl, sender, msg) => LogUI($"[{lvl}] {sender}: {msg}");
        }

        void OnDisable()
        {
            LiftoffWindows.OnDiagnostic -= (lvl, s, m) => { };
        }

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

            bool ok = LiftoffWindows.Initialize(appId, hwnd);
            LogUI($"[Liftoff] Initialize returned {ok}. WebView2 available: {LiftoffWindows.IsWebView2Available()}");
#else
            LogUI("[Liftoff] Initialize: non-Windows platform.");
#endif
        }

        public void OnLoadClicked()
        {
            bool ok = LiftoffWindows.LoadAd(placement);
            LogUI($"[Liftoff] LoadAd('{placement}') returned {ok}");
        }

        public void OnPlayClicked()
        {
            StartCoroutine(PlayNextFrame());
        }

        IEnumerator PlayNextFrame()
        {
            yield return null; // next frame on main thread
            bool ok = LiftoffWindows.PlayAd(placement);
            LogUI($"[Liftoff] PlayAd('{placement}') returned {ok}");
        }

        void OnApplicationQuit()
        {
            LiftoffWindows.Shutdown();
        }
    }
}
