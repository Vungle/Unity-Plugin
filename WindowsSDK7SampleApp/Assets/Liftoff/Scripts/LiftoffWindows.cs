using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT; // MonoPInvokeCallbackAttribute

namespace Liftoff.Windows
{
    public static class LiftoffWindows
    {
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

        // AOT-stable delegate instances to static methods
        static readonly Native.InitSuccessCB _cbInitOk = InitOkTrampoline;
        static readonly Native.InitFailureCB _cbInitFail = InitFailTrampoline;
        static readonly Native.AdLoadSuccessCB _cbLoadOk = LoadOkTrampoline;
        static readonly Native.AdLoadFailureCB _cbLoadFail = LoadFailTrampoline;
        static readonly Native.AdStartCB _cbAdStart = AdStartTrampoline;
        static readonly Native.AdEndCB _cbAdEnd = AdEndTrampoline;
        static readonly Native.AdPlayFailureCB _cbAdPlayFail = AdPlayFailTrampoline;
        static readonly Native.AdRewardedCB _cbRewarded = AdRewardedTrampoline;
        static readonly Native.AdClickCB _cbClick = AdClickTrampoline;
        static readonly Native.DiagnosticCB _cbDiag = DiagnosticTrampoline;

        static bool _callbacksInstalled;

        // Trampolines — static, attributed, no captures
        [MonoPInvokeCallback(typeof(Native.InitSuccessCB))]
        static void InitOkTrampoline() =>
            LiftoffMainThread.Post(() => OnInitialized?.Invoke());

        [MonoPInvokeCallback(typeof(Native.InitFailureCB))]
        static void InitFailTrampoline(int code, string message) =>
            LiftoffMainThread.Post(() => OnInitializationFailed?.Invoke(code, message));

        [MonoPInvokeCallback(typeof(Native.AdLoadSuccessCB))]
        static void LoadOkTrampoline(string placement) =>
            LiftoffMainThread.Post(() => OnAdLoaded?.Invoke(placement));

        [MonoPInvokeCallback(typeof(Native.AdLoadFailureCB))]
        static void LoadFailTrampoline(string placement, int code, string message) =>
            LiftoffMainThread.Post(() => OnAdLoadFailed?.Invoke(placement, code, message));

        [MonoPInvokeCallback(typeof(Native.AdStartCB))]
        static void AdStartTrampoline(string placement, string eventId) =>
            LiftoffMainThread.Post(() => OnAdStart?.Invoke(placement, eventId));

        [MonoPInvokeCallback(typeof(Native.AdEndCB))]
        static void AdEndTrampoline(string placement) =>
            LiftoffMainThread.Post(() => OnAdEnd?.Invoke(placement));

        [MonoPInvokeCallback(typeof(Native.AdPlayFailureCB))]
        static void AdPlayFailTrampoline(string placement, int code, string message) =>
            LiftoffMainThread.Post(() => OnAdPlayFailed?.Invoke(placement, code, message));

        [MonoPInvokeCallback(typeof(Native.AdRewardedCB))]
        static void AdRewardedTrampoline(string placement) =>
            LiftoffMainThread.Post(() => OnAdRewarded?.Invoke(placement));

        [MonoPInvokeCallback(typeof(Native.AdClickCB))]
        static void AdClickTrampoline(string placement) =>
            LiftoffMainThread.Post(() => OnAdClick?.Invoke(placement));

        [MonoPInvokeCallback(typeof(Native.DiagnosticCB))]
        static void DiagnosticTrampoline(int level, string senderType, string message) =>
            LiftoffMainThread.Post(() => OnDiagnostic?.Invoke(level, senderType, message));

        static LiftoffWindows()
        {
            var cbs = new Native.BridgeCallbacks
            {
                initSuccess = _cbInitOk,
                initFailure = _cbInitFail,
                loadSuccess = _cbLoadOk,
                loadFailure = _cbLoadFail,
                adStart = _cbAdStart,
                adEnd = _cbAdEnd,
                adPlayFailure = _cbAdPlayFail,
                adRewarded = _cbRewarded,
                adClick = _cbClick
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

            try { Native.Liftoff_SetDiagnosticCallback(_cbDiag); }
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
