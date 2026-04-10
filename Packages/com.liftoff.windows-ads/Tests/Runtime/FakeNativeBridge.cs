using System;
using System.Runtime.InteropServices;

namespace Liftoff.Windows.Tests
{
    internal class FakeNativeBridge : INativeBridge
    {
        // Initialize
        public bool InitializeCalled;
        public string LastAppId;
        public IntPtr LastHwnd;
        public bool InitializeReturn = true;

        // IsInitialized
        public bool IsInitializedReturn;

        // LoadAd
        public bool LoadAdCalled;
        public string LastLoadPlacement;
        public bool LoadAdReturn = true;

        // LoadAdWithMarkup
        public bool LoadAdWithMarkupCalled;
        public string LastLoadMarkup;
        public bool LoadAdWithMarkupReturn = true;

        // PlayAd
        public bool PlayAdCalled;
        public string LastPlayPlacement;
        public bool PlayAdReturn = true;

        // PlayAdWithMarkup
        public bool PlayAdWithMarkupCalled;
        public string LastPlayMarkup;
        public bool PlayAdWithMarkupReturn = true;

        // IsWebView2Available
        public bool IsWebView2Return = true;

        // Shutdown
        public bool ShutdownCalled;

        // Privacy
        public bool SetCoppaCalled;
        public bool LastCoppaStatus;

        public bool SetCcpaCalled;
        public int LastCcpaStatus;

        public bool SetGdprCalled;
        public int LastGdprStatus;
        public string LastGdprVersion;

        public bool SetAshwidCalled;
        public bool LastAshwidDisabled;

        // SuperToken
        public bool GetSuperTokenCalled;
        public string LastSuperTokenPlacement;
        public string SuperTokenResult;

        public bool Initialize(string appId, IntPtr hwnd)
        {
            InitializeCalled = true;
            LastAppId = appId;
            LastHwnd = hwnd;
            return InitializeReturn;
        }

        public bool IsInitialized() => IsInitializedReturn;

        public bool LoadAd(string placement)
        {
            LoadAdCalled = true;
            LastLoadPlacement = placement;
            return LoadAdReturn;
        }

        public bool LoadAdWithMarkup(string placement, string markup)
        {
            LoadAdWithMarkupCalled = true;
            LastLoadPlacement = placement;
            LastLoadMarkup = markup;
            return LoadAdWithMarkupReturn;
        }

        public bool PlayAd(string placement)
        {
            PlayAdCalled = true;
            LastPlayPlacement = placement;
            return PlayAdReturn;
        }

        public bool PlayAdWithMarkup(string placement, string markup)
        {
            PlayAdWithMarkupCalled = true;
            LastPlayPlacement = placement;
            LastPlayMarkup = markup;
            return PlayAdWithMarkupReturn;
        }

        public bool IsWebView2Available() => IsWebView2Return;

        public void Shutdown() { ShutdownCalled = true; }

        public void SetCoppaStatus(bool status)
        {
            SetCoppaCalled = true;
            LastCoppaStatus = status;
        }

        public void SetCcpaStatus(int status)
        {
            SetCcpaCalled = true;
            LastCcpaStatus = status;
        }

        public void SetGdprConsentStatus(int status, string version)
        {
            SetGdprCalled = true;
            LastGdprStatus = status;
            LastGdprVersion = version;
        }

        public void SetDisableAshwidTracking(bool disabled)
        {
            SetAshwidCalled = true;
            LastAshwidDisabled = disabled;
        }

        public IntPtr GetSuperToken(string placement)
        {
            GetSuperTokenCalled = true;
            LastSuperTokenPlacement = placement;
            if (SuperTokenResult == null) return IntPtr.Zero;
            IntPtr ptr = Marshal.StringToCoTaskMemUni(SuperTokenResult);
            return ptr;
        }

        public void Reset()
        {
            InitializeCalled = false;
            LastAppId = null;
            LastHwnd = IntPtr.Zero;
            InitializeReturn = true;
            IsInitializedReturn = false;
            LoadAdCalled = false;
            LastLoadPlacement = null;
            LoadAdReturn = true;
            LoadAdWithMarkupCalled = false;
            LastLoadMarkup = null;
            LoadAdWithMarkupReturn = true;
            PlayAdCalled = false;
            LastPlayPlacement = null;
            PlayAdReturn = true;
            PlayAdWithMarkupCalled = false;
            LastPlayMarkup = null;
            PlayAdWithMarkupReturn = true;
            IsWebView2Return = true;
            ShutdownCalled = false;
            SetCoppaCalled = false;
            LastCoppaStatus = false;
            SetCcpaCalled = false;
            LastCcpaStatus = 0;
            SetGdprCalled = false;
            LastGdprStatus = 0;
            LastGdprVersion = null;
            SetAshwidCalled = false;
            LastAshwidDisabled = false;
            GetSuperTokenCalled = false;
            LastSuperTokenPlacement = null;
            SuperTokenResult = null;
        }
    }
}
