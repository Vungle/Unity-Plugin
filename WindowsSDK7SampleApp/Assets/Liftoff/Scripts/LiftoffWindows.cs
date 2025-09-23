using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Liftoff.Windows
{
    /// <summary>
    /// Platform-safe Unity wrapper.
    /// On Windows Standalone / Windows Editor it P/Invokes the native DLL.
    /// On other platforms (iOS/Android/etc.) it exposes the same API as no-ops so your game compiles.
    /// </summary>
    public static class LiftoffWindows
    {
        // ----- Public events (available on all platforms) -----
        public static event Action OnInitialized;
        public static event Action<int, string> OnInitializationFailed;

        public static event Action<string> OnAdLoaded;
        public static event Action<string, int, string> OnAdLoadFailed;

        public static event Action<string, string> OnAdStart;
        public static event Action<string> OnAdEnd;
        public static event Action<string, int, string> OnAdPlayFailed;
        public static event Action<string> OnAdRewarded;
        public static event Action<string> OnAdClick;
		public static event Action<int, string, string> OnDiagnostic;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		private static class Native
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void InitSuccessCB();
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void InitFailureCB(int code, [MarshalAs(UnmanagedType.LPWStr)] string message);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdLoadSuccessCB([MarshalAs(UnmanagedType.LPWStr)] string placement);
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdLoadFailureCB([MarshalAs(UnmanagedType.LPWStr)] string placement, int code, [MarshalAs(UnmanagedType.LPWStr)] string message);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdStartCB([MarshalAs(UnmanagedType.LPWStr)] string placement, [MarshalAs(UnmanagedType.LPWStr)] string eventId);
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdEndCB([MarshalAs(UnmanagedType.LPWStr)] string placement);
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdPlayFailureCB([MarshalAs(UnmanagedType.LPWStr)] string placement, int code, [MarshalAs(UnmanagedType.LPWStr)] string message);
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdRewardedCB([MarshalAs(UnmanagedType.LPWStr)] string placement);
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdClickCB([MarshalAs(UnmanagedType.LPWStr)] string placement);
			[UnmanagedFunctionPointer(CallingConvention.StdCall)]
			public delegate void DiagnosticCB(int level, [MarshalAs(UnmanagedType.LPWStr)] string senderType, [MarshalAs(UnmanagedType.LPWStr)] string message);

			[StructLayout(LayoutKind.Sequential)]
            public struct BridgeCallbacks
            {
                public InitSuccessCB initSuccess;
                public InitFailureCB initFailure;
                public AdLoadSuccessCB loadSuccess;
                public AdLoadFailureCB loadFailure;
                public AdStartCB adStart;
                public AdEndCB adEnd;
                public AdPlayFailureCB adPlayFailure;
                public AdRewardedCB adRewarded;
                public AdClickCB adClick;
            }

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern void Liftoff_SetCallbacks(BridgeCallbacks cbs);

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern bool Liftoff_Initialize(string appId, IntPtr hwnd);

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern bool Liftoff_IsInitialized();

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern bool Liftoff_LoadAd(string placement);

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern bool Liftoff_PlayAd(string placement);

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern void Liftoff_Shutdown();

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern bool Liftoff_IsWebView2Available();

			[DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
			public static extern void Liftoff_SetDiagnosticCallback(DiagnosticCB cb);

			[DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
			public static extern void Liftoff_ClearDiagnosticCallback();
		}

        // Hold delegates to prevent GC
        static Native.InitSuccessCB _iok;
        static Native.InitFailureCB _ifail;
        static Native.AdLoadSuccessCB _ldok;
        static Native.AdLoadFailureCB _ldfail;
        static Native.AdStartCB _start;
        static Native.AdEndCB _end;
        static Native.AdPlayFailureCB _pfail;
        static Native.AdRewardedCB _rewarded;
        static Native.AdClickCB _click;
		static Native.DiagnosticCB _diag;

		static LiftoffWindows()
        {
            _iok = () => OnInitialized?.Invoke();
            _ifail = (code, msg) => OnInitializationFailed?.Invoke(code, msg);

            _ldok = (placement) => OnAdLoaded?.Invoke(placement);
            _ldfail = (placement, code, msg) => OnAdLoadFailed?.Invoke(placement, code, msg);

            _start = (placement, eventId) => OnAdStart?.Invoke(placement, eventId);
            _end = (placement) => OnAdEnd?.Invoke(placement);
            _pfail = (placement, code, msg) => OnAdPlayFailed?.Invoke(placement, code, msg);
            _rewarded = (placement) => OnAdRewarded?.Invoke(placement);
            _click = (placement) => OnAdClick?.Invoke(placement);
			_diag = (level, sender, msg) => OnDiagnostic?.Invoke(level, sender, msg);

			var cbs = new Native.BridgeCallbacks {
                initSuccess = _iok,
                initFailure = _ifail,
                loadSuccess  = _ldok,
                loadFailure  = _ldfail,
                adStart      = _start,
                adEnd        = _end,
                adPlayFailure= _pfail,
                adRewarded   = _rewarded,
                adClick      = _click
            };
            Native.Liftoff_SetCallbacks(cbs);
			try
			{
				Native.Liftoff_SetDiagnosticCallback(_diag);
			}
			catch (System.DllNotFoundException e)
			{
				Debug.LogError("[Liftoff] Diagnostic hook failed (DLL missing): " + e);
			}
			catch (System.EntryPointNotFoundException e)
			{
				Debug.LogError("[Liftoff] Diagnostic hook failed (export missing): " + e);
			}
		}

        public static bool Initialize(string appId, IntPtr hwnd) => Native.Liftoff_Initialize(appId, hwnd);
        public static bool IsInitialized => Native.Liftoff_IsInitialized();
        public static bool LoadAd(string placement) => Native.Liftoff_LoadAd(placement);
        public static bool PlayAd(string placement) => Native.Liftoff_PlayAd(placement);
        public static bool IsWebView2Available() => Native.Liftoff_IsWebView2Available();
        public static void Shutdown()
        {
            try { Native.Liftoff_ClearDiagnosticCallback(); } catch { }
            Native.Liftoff_Shutdown();
        }

#else
        // ---------- Non-Windows platforms: Stubs ----------
        static LiftoffWindows()
        {
            // nothing to wire
        }

        public static bool Initialize(string appId, IntPtr hwnd)
        {
            Debug.LogWarning("[Liftoff] LiftoffWindows.Initialize called on non-Windows platform. This is a no-op.");
            // simulate async success for editor play on non-Windows?
            return false;
        }

        public static bool IsInitialized => false;
        public static bool LoadAd(string placement)
        {
            Debug.LogWarning("[Liftoff] LoadAd called on non-Windows platform. No-op.");
            return false;
        }
        public static bool PlayAd(string placement)
        {
            Debug.LogWarning("[Liftoff] PlayAd called on non-Windows platform. No-op.");
            return false;
        }
        public static bool IsWebView2Available() => false;
        public static void Shutdown() {}
#endif
    }
}
