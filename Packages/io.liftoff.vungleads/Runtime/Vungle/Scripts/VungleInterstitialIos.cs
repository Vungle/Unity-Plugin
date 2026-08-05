using System;
using System.Runtime.InteropServices;
using AOT;

namespace VungleAds
{
    #if UNITY_IOS

    public partial class VungleInterstitial 
    {
        // This is Obj-C reference so we know which object to call the API on
        private IntPtr interstitialPtr = IntPtr.Zero;
        // This is passed to the Obj-C wrapper class so it can reference the Unity object in the callbacks
        private IntPtr unityInterstitialPtr = IntPtr.Zero;
        private bool canPlay = false;

        internal delegate void VungleInterstitialAdLoadedCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdFailedToLoadCallback(IntPtr interstitialAd, string error);
        internal delegate void VungleInterstitialAdWillPresentCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdDidPresentCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdFailedToPresentCallback(IntPtr interstitialAd, string error);
        internal delegate void VungleInterstitialAdWillCloseCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdDidCloseCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdDidTrackImpressionCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdDidClickCallback(IntPtr interstitialAd);
        internal delegate void VungleInterstitialAdWillLeaveApplicationCallback(IntPtr interstitialAd);

        public VungleInterstitial(string placementId)
        {
            this.placementId = placementId;
            this.BuildInterstitial();   
        }

        public void BuildInterstitial()
        {
            this.unityInterstitialPtr = (IntPtr)GCHandle.Alloc(this);
            this.interstitialPtr = CreateVungleInterstitialAd(this.unityInterstitialPtr, this.placementId);
            SetVungleInterstitialAdCallbacks(this.interstitialPtr,
               InterstitialLoadedCallback,
               InterstitialFailedToLoadCallback,
               InterstitialWillPresentCallback,
               InterstitialDidPresentCallback,
               InterstitialFailedToPresentCallback,
               InterstitialWillCloseCallback,
               InterstitialDidCloseCallback,
               InterstitialDidTrackImpressionCallback,
               InterstitialDidClickCallback,
               InterstitialWillLeaveApplicationCallback);
        }

        ~VungleInterstitial()
        {
            interstitialPtr = IntPtr.Zero;
            if (unityInterstitialPtr != IntPtr.Zero)
            {
                ((GCHandle)unityInterstitialPtr).Free();
                unityInterstitialPtr = IntPtr.Zero;
            }
        }

        public bool CanPlay()
        {
            return canPlay && interstitialPtr != IntPtr.Zero;
        }

        public void Load()
        {
            if (interstitialPtr == IntPtr.Zero) return;
            LoadVungleInterstitialAd(this.interstitialPtr);
        }

        public void LoadWithCsbData(VungleCSBData csbData)
        {
            if (interstitialPtr == IntPtr.Zero || csbData == null) return;
            int count = csbData.Extras != null ? csbData.Extras.Count : 0;
            string[] keys = count > 0 ? new string[count] : null;
            string[] values = count > 0 ? new string[count] : null;
            if (count > 0)
            {
                int i = 0;
                foreach (var kvp in csbData.Extras) { keys[i] = kvp.Key; values[i] = kvp.Value; i++; }
            }
            LoadVungleInterstitialAdWithCsb(this.interstitialPtr, csbData.BidFloor, csbData.AuctionId, csbData.CreativeId, csbData.AdUnitId, csbData.IsVxWinner, csbData.IsPriorityAccess, keys, values, count);
        }

        public double GetWinningPrice()
        {
            if (interstitialPtr == IntPtr.Zero) return 0;
            return GetVungleInterstitialAdWinningPrice(this.interstitialPtr);
        }

        public void SendWinURL()
        {
            if (interstitialPtr == IntPtr.Zero) return;
            SendVungleInterstitialAdWinURL(this.interstitialPtr);
        }

        public void SendLossURL()
        {
            if (interstitialPtr == IntPtr.Zero) return;
            SendVungleInterstitialAdLossURL(this.interstitialPtr);
        }

        public void Show()
        {
            if (interstitialPtr == IntPtr.Zero) return;
            canPlay = false;
            ShowVungleInterstitialAd(this.interstitialPtr);
        }

        #region Interstitial Callbacks

        [MonoPInvokeCallback(typeof(VungleInterstitialAdLoadedCallback))]
        private static void InterstitialLoadedCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.canPlay = true;
            client.onLoadSuccess?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdFailedToLoadCallback))]
        private static void InterstitialFailedToLoadCallback(IntPtr interstitialAd, string error)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onLoadFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdWillPresentCallback))]
        private static void InterstitialWillPresentCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onWillPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdDidPresentCallback))]
        private static void InterstitialDidPresentCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onDidPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdFailedToPresentCallback))]
        private static void InterstitialFailedToPresentCallback(IntPtr interstitialAd, string error)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onPresentFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdWillCloseCallback))]
        private static void InterstitialWillCloseCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onWillClose?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdDidCloseCallback))]
        private static void InterstitialDidCloseCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onDidClose?.Invoke();
            client.interstitialPtr = IntPtr.Zero;
            if (client.unityInterstitialPtr != IntPtr.Zero)
            {
                ((GCHandle)client.unityInterstitialPtr).Free();
                client.unityInterstitialPtr = IntPtr.Zero;
            }
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdDidTrackImpressionCallback))]
        private static void InterstitialDidTrackImpressionCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onImpression?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdDidClickCallback))]
        private static void InterstitialDidClickCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onClick?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleInterstitialAdWillLeaveApplicationCallback))]
        private static void InterstitialWillLeaveApplicationCallback(IntPtr interstitialAd)
        {
            VungleInterstitial client = IntPtrToInterstitial(interstitialAd);
            if (client == null) return;
            client.onWillLeaveApplication?.Invoke();
        }

        private static VungleInterstitial IntPtrToInterstitial(IntPtr interstitialAd)
        {
            GCHandle handle = (GCHandle)interstitialAd;
            return handle.Target as VungleInterstitial;
        }

        #endregion

        #region Interstitial Externs

        [DllImport("__Internal")]
        internal static extern IntPtr CreateVungleInterstitialAd(IntPtr unityRef, string placementId);

        [DllImport("__Internal")]
        internal static extern void SetVungleInterstitialAdCallbacks(IntPtr interstitialAd,
            VungleInterstitialAdLoadedCallback adLoadedCallback,
            VungleInterstitialAdFailedToLoadCallback adFailedToLoadCallback,
            VungleInterstitialAdLoadedCallback adWillPresentCallback,
            VungleInterstitialAdLoadedCallback adDidPresentCallback,
            VungleInterstitialAdFailedToLoadCallback adFailedToPresentCallback,
            VungleInterstitialAdLoadedCallback adWillCloseCallback,
            VungleInterstitialAdLoadedCallback adDidCloseCallback,
            VungleInterstitialAdLoadedCallback adDidTrackImpressionCallback,
            VungleInterstitialAdLoadedCallback adDidClickCallback,
            VungleInterstitialAdLoadedCallback adWillLeaveApplicationCallback);

        [DllImport("__Internal")]
        internal static extern void LoadVungleInterstitialAd(IntPtr interstitialAd);

        [DllImport("__Internal")]
        internal static extern void LoadVungleInterstitialAdWithCsb(IntPtr interstitialAd, double bidFloor, string auctionId, string creativeId, string adUnitId, bool isVxWinner, bool isPriorityAccess, string[] extrasKeys, string[] extrasValues, int extrasCount);

        [DllImport("__Internal")]
        internal static extern void ShowVungleInterstitialAd(IntPtr interstitialAd);

        [DllImport("__Internal")]
        internal static extern double GetVungleInterstitialAdWinningPrice(IntPtr interstitialAd);

        [DllImport("__Internal")]
        internal static extern void SendVungleInterstitialAdWinURL(IntPtr interstitialAd);

        [DllImport("__Internal")]
        internal static extern void SendVungleInterstitialAdLossURL(IntPtr interstitialAd);

        #endregion
    }

    #endif
}
