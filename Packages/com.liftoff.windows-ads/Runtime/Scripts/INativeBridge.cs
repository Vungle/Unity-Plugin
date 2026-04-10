using System;

namespace Liftoff.Windows
{
    internal interface INativeBridge
    {
        bool Initialize(string appId, IntPtr hwnd);
        bool IsInitialized();
        bool LoadAd(string placement);
        bool LoadAdWithMarkup(string placement, string markup);
        bool PlayAd(string placement);
        bool PlayAdWithMarkup(string placement, string markup);
        bool IsWebView2Available();
        void Shutdown();
        void SetCoppaStatus(bool status);
        void SetCcpaStatus(int status);
        void SetGdprConsentStatus(int status, string version);
        void SetDisableAshwidTracking(bool disabled);
        IntPtr GetSuperToken(string placement);
    }
}
