// LiftoffWindows.cs (drop-in replacement)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Liftoff.Windows
{
    public static class LiftoffWindows
    {
        // Public events
        public static event Action OnInitialized;
        public static event Action<int, string> OnInitializationFailed;
        public static event Action<string> OnAdLoaded;
        public static event Action<string, int, string> OnAdLoadFailed;
        public static event Action<string, string> OnAdStart;
        public static event Action<string> OnAdEnd;
        public static event Action<string, int, string> OnAdPlayFailed;
        public static event Action<string> OnAdRewarded;
        public static event Action<string> OnAdClick;
        public static event Action<int, string, string> OnDiagnostic; // level, sender, message

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private static class Native
        {
            // Callback delegates
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

            // P/Invoke
            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern void Liftoff_SetCallbacks(BridgeCallbacks cbs);

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern void Liftoff_SetDiagnosticCallback(DiagnosticCB cb);

            [DllImport("LiftoffUnityBridge", CallingConvention = CallingConvention.StdCall)]
            public static extern void Liftoff_ClearDiagnosticCallback();

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
        }

        // Keep delegates alive
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

        static bool _callbacksInstalled;

        static LiftoffWindows()
        {
            // Wire managed -> managed events
            _iok = () => OnInitialized?.Invoke();
            _ifail = (c, m) => OnInitializationFailed?.Invoke(c, m);
            _ldok = p => OnAdLoaded?.Invoke(p);
            _ldfail = (p, c, m) => OnAdLoadFailed?.Invoke(p, c, m);
            _start = (p, eid) => OnAdStart?.Invoke(p, eid);
            _end = p => OnAdEnd?.Invoke(p);
            _pfail = (p, c, m) => OnAdPlayFailed?.Invoke(p, c, m);
            _rewarded = p => OnAdRewarded?.Invoke(p);
            _click = p => OnAdClick?.Invoke(p);

            var cbs = new Native.BridgeCallbacks
            {
                initSuccess = _iok,
                initFailure = _ifail,
                loadSuccess = _ldok,
                loadFailure = _ldfail,
                adStart = _start,
                adEnd = _end,
                adPlayFailure = _pfail,
                adRewarded = _rewarded,
                adClick = _click
            };

            try
            {
                Native.Liftoff_SetCallbacks(cbs);
                _callbacksInstalled = true;
            }
            catch (DllNotFoundException e) { Debug.LogError("[Liftoff] SetCallbacks DllNotFound: " + e.Message); }
            catch (EntryPointNotFoundException e) { Debug.LogError("[Liftoff] SetCallbacks EntryPointNotFound: " + e.Message); }
            catch (BadImageFormatException e) { Debug.LogError("[Liftoff] SetCallbacks BadImageFormat: " + e.Message); }
            catch (Exception e) { Debug.LogError("[Liftoff] SetCallbacks unexpected: " + e); }

            _diag = (level, sender, message) => OnDiagnostic?.Invoke(level, sender, message);

            try { Native.Liftoff_SetDiagnosticCallback(_diag); }
            catch (DllNotFoundException e) { Debug.LogError("[Liftoff] Diagnostic DllNotFound: " + e.Message); }
            catch (EntryPointNotFoundException e) { Debug.LogError("[Liftoff] Diagnostic EntryPointNotFound: " + e.Message); }
            catch (BadImageFormatException e) { Debug.LogError("[Liftoff] Diagnostic BadImageFormat: " + e.Message); }
            catch (Exception e) { Debug.LogError("[Liftoff] Diagnostic unexpected: " + e); }
        }
#endif // UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        public static bool Initialize(string appId, IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return Native.Liftoff_Initialize(appId, hwnd);
#else
            Debug.Log("[Liftoff] Initialize: non-Windows platform (no-op).");
            return false;
#endif
        }

        public static bool IsInitialized
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return Native.Liftoff_IsInitialized();
#else
                return false;
#endif
            }
        }

        public static bool LoadAd(string placement)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return Native.Liftoff_LoadAd(placement);
#else
            Debug.Log("[Liftoff] LoadAd: non-Windows platform (no-op).");
            return false;
#endif
        }

        public static bool PlayAd(string placement)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return Native.Liftoff_PlayAd(placement);
#else
            Debug.Log("[Liftoff] PlayAd: non-Windows platform (no-op).");
            return false;
#endif
        }

        public static bool IsWebView2Available()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return Native.Liftoff_IsWebView2Available();
#else
            return false;
#endif
        }

        public static void Shutdown()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try { Native.Liftoff_ClearDiagnosticCallback(); } catch { }

            if (_callbacksInstalled)
            {
                try
                {
                    Native.BridgeCallbacks zero = default;
                    Native.Liftoff_SetCallbacks(zero);
                }
                catch { /* ignore */ }
                _callbacksInstalled = false;
            }

            try { Native.Liftoff_Shutdown(); } catch { }
#else
            // no-op elsewhere
#endif
        }
    }
}
