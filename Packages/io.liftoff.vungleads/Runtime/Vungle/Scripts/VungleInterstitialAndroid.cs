using System;
using System.Collections.Generic;
using UnityEngine;

namespace VungleAds
{
    #if UNITY_ANDROID

    public partial class VungleInterstitial : AndroidJavaProxy
    {
        private AndroidJavaObject interstitialObject;

        public VungleInterstitial(string placementId) : base("com.vungle.androidplugin.IVungleInterstitialCallbackReceiver")
        {
            this.placementId = placementId;
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginInterstitialAd");
            interstitialObject = vungleClass.CallStatic<AndroidJavaObject>("createInstance", this.placementId, this);
        }

        ~VungleInterstitial()
        {
            interstitialObject?.Dispose();
            interstitialObject = null;
        }

        public bool CanPlay()
        {
            if (interstitialObject == null) return false;
            return interstitialObject.Call<bool>("canPlayAd");
        }

        public void Load()
        {
            if (interstitialObject == null) return;
            interstitialObject.Call("loadAd");
        }

        public void LoadWithCsbData(VungleCSBData csbData)
        {
            if (interstitialObject == null || csbData == null) return;
            int count = csbData.Extras != null ? csbData.Extras.Count : 0;
            string[] keys = count > 0 ? new string[count] : null;
            string[] values = count > 0 ? new string[count] : null;
            if (count > 0)
            {
                int i = 0;
                foreach (var kvp in csbData.Extras) { keys[i] = kvp.Key; values[i] = kvp.Value; i++; }
            }
            interstitialObject.Call("loadAdWithCsb", csbData.BidFloor, csbData.AuctionId, csbData.CreativeId, csbData.AdUnitId, csbData.IsVxWinner, csbData.IsPriorityAccess, keys, values, count);
        }

        public double GetWinningPrice()
        {
            if (interstitialObject == null) return 0;
            return interstitialObject.Call<double>("getWinningPrice");
        }

        public void SendWinURL()
        {
            if (interstitialObject == null) return;
            interstitialObject.Call("sendWinURL");
        }

        public void SendLossURL()
        {
            if (interstitialObject == null) return;
            interstitialObject.Call("sendLossURL");
        }

        public void Show()
        {
            if (interstitialObject == null) return;
            interstitialObject.Call("playAd");
        }

        #region Interstitial Callbacks

        public void InterstitialLoadedCallback() => VungleThreadDispatcher.Enqueue(() => onLoadSuccess?.Invoke());
        public void InterstitialFailedToLoadCallback(String error) => VungleThreadDispatcher.Enqueue(() => onLoadFailed?.Invoke(error));
        public void InterstitialDidPresentCallback() => VungleThreadDispatcher.Enqueue(() => onDidPresent?.Invoke());
        public void InterstitialFailedToPresentCallback(String error) => VungleThreadDispatcher.Enqueue(() => onPresentFailed?.Invoke(error));
        public void InterstitialDidCloseCallback() => VungleThreadDispatcher.Enqueue(() => onDidClose?.Invoke());
        public void InterstitialDidTrackImpressionCallback() => VungleThreadDispatcher.Enqueue(() => onImpression?.Invoke());
        public void InterstitialDidClickCallback() => VungleThreadDispatcher.Enqueue(() => onClick?.Invoke());
        public void InterstitialWillLeaveApplicationCallback() => VungleThreadDispatcher.Enqueue(() => onWillLeaveApplication?.Invoke());

        #endregion
    }

    #endif
}
