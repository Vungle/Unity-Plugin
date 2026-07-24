using System;
using System.Collections.Generic;
using UnityEngine;

namespace VungleAds
{
    #if UNITY_ANDROID

    public partial class VungleRewarded : AndroidJavaProxy
    {
        private AndroidJavaObject rewardedObject;

        public VungleRewarded(string placementId) : base("com.vungle.androidplugin.IVungleRewardedCallbackReceiver")
        {
            this.placementId = placementId;
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VunglePluginRewardedAd");
            rewardedObject = vungleClass.CallStatic<AndroidJavaObject>("createInstance", this.placementId, this);
        }

        ~VungleRewarded()
        {
            rewardedObject?.Dispose();
            rewardedObject = null;
        }

        public bool CanPlay()
        {
            if (rewardedObject == null) return false;
            return rewardedObject.Call<bool>("canPlay");
        }

        public void Load()
        {
            if (rewardedObject == null) return;
            rewardedObject.Call("loadAd");
        }

        public void LoadWithCsbData(VungleCSBData csbData)
        {
            if (rewardedObject == null || csbData == null) return;
            int count = csbData.Extras != null ? csbData.Extras.Count : 0;
            string[] keys = count > 0 ? new string[count] : null;
            string[] values = count > 0 ? new string[count] : null;
            if (count > 0)
            {
                int i = 0;
                foreach (var kvp in csbData.Extras) { keys[i] = kvp.Key; values[i] = kvp.Value; i++; }
            }
            rewardedObject.Call("loadAdWithCsb", csbData.BidFloor, csbData.AuctionId, csbData.CreativeId, csbData.AdUnitId, csbData.IsVxWinner, csbData.IsPriorityAccess, keys, values, count);
        }

        public double GetWinningPrice()
        {
            if (rewardedObject == null) return 0;
            return rewardedObject.Call<double>("getWinningPrice");
        }

        public void SendWinURL()
        {
            if (rewardedObject == null) return;
            rewardedObject.Call("sendWinURL");
        }

        public void SendLossURL()
        {
            if (rewardedObject == null) return;
            rewardedObject.Call("sendLossURL");
        }

        public void Show()
        {
            if (rewardedObject == null) return;
            rewardedObject.Call("playAd");
        }

        #region Rewarded Callbacks

        public void RewardedLoadedCallback() => VungleThreadDispatcher.Enqueue(() => onLoadSuccess?.Invoke());
        public void RewardedFailedToLoadCallback(String error) => VungleThreadDispatcher.Enqueue(() => onLoadFailed?.Invoke(error));
        public void RewardedDidPresentCallback() => VungleThreadDispatcher.Enqueue(() => onDidPresent?.Invoke());
        public void RewardedFailedToPresentCallback(String error) => VungleThreadDispatcher.Enqueue(() => onPresentFailed?.Invoke(error));
        public void RewardedDidCloseCallback() => VungleThreadDispatcher.Enqueue(() => onDidClose?.Invoke());
        public void RewardedDidTrackImpressionCallback() => VungleThreadDispatcher.Enqueue(() => onImpression?.Invoke());
        public void RewardedDidClickCallback() => VungleThreadDispatcher.Enqueue(() => onClick?.Invoke());
        public void RewardedWillLeaveApplicationCallback() => VungleThreadDispatcher.Enqueue(() => onWillLeaveApplication?.Invoke());
        public void RewardedDidRewardUserCallback() => VungleThreadDispatcher.Enqueue(() => onDidRewardUser?.Invoke());

        #endregion
    }

    #endif
}
