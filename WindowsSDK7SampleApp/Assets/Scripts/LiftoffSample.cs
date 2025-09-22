using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Liftoff.Windows;
using TMPro;

public class LiftoffSample : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
	[DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
#endif

	[Header("Liftoff App ID")]
	public string appId = "59792a4f057243276200298a";
	[Header("Placement")]
	public string placement = "DEFAULT18154";
	public TMP_Text text;

	void LogUI(string msg)
	{
		Debug.Log(msg);
		if (text != null) text.text = msg + "\n" + text.text;
	}

	private void Awake()
	{
		LiftoffWindows.OnInitialized += () => LogUI("[Liftoff] Initialized.");
		LiftoffWindows.OnInitializationFailed += (c, m) => LogUI($"[Liftoff] Init failed {c}: {m}");
		LiftoffWindows.OnAdLoaded += p => LogUI($"[Liftoff] Loaded: {p}.");
		LiftoffWindows.OnAdLoadFailed += (p, c, m) => LogUI($"[Liftoff] Load fail {p}: {c} {m}");
		LiftoffWindows.OnAdStart += (p, eid) => LogUI($"[Liftoff] Start {p} eid={eid}");
		LiftoffWindows.OnAdEnd += p => LogUI($"[Liftoff] End {p}");
		LiftoffWindows.OnAdPlayFailed += (p, c, m) => LogUI($"[Liftoff] Play fail {p}: {c} {m}");
		LiftoffWindows.OnAdRewarded += p => LogUI($"[Liftoff] Rewarded {p}");
		LiftoffWindows.OnAdClick += p => LogUI($"[Liftoff] Click {p}");
		LiftoffWindows.OnDiagnostic += (level, sender, msg) => LogUI($"{DateTime.Now:HH:mm:ss} [{sender}] {msg}");
	}

	private void Start()
	{
		LogUI($"[Liftoff] Start on {Application.platform}, editor={Application.isEditor}");
	}

	private void OnDestroy() => LiftoffWindows.Shutdown();

	private System.Collections.IEnumerator PlayNextFrame()
	{
		yield return null; // main thread, next frame
		var ok = LiftoffWindows.PlayAd(placement);
		LogUI($"[Liftoff] PlayAd returned {ok}");
	}

	public void OnInitClicked()
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		IntPtr hwnd = IntPtr.Zero;
		try { hwnd = GetActiveWindow(); } catch (Exception e) { LogUI($"[Liftoff] GetActiveWindow failed: {e.Message}"); }
		if (hwnd == IntPtr.Zero)
		{
			try { hwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle; }
			catch (Exception e) { LogUI($"[Liftoff] MainWindowHandle failed: {e.Message}"); }
		}

		if (!LiftoffWindows.IsWebView2Available())
			LogUI("[Liftoff] WebView2 Runtime not detected (install Evergreen).");

		LogUI("[Liftoff] Initializing…");
		LiftoffWindows.Initialize(appId, hwnd);
#else
        LogUI("[Liftoff] Non-Windows platform: wrapper is a no-op here.");
#endif
	}

	public void OnLoadClicked()
	{
		bool b = LiftoffWindows.LoadAd(placement);
		LogUI($"[Liftoff] Load Result {b}");
	}

	public void OnPlayClicked()
	{
		StartCoroutine(PlayNextFrame());
	}
}
