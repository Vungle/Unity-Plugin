using System;
using System.Runtime.InteropServices;
using AOT;

namespace VungleAds
{
    #if UNITY_IOS

    public partial class VungleRewarded
    {
        // This is Obj-C reference so we know which object to call the API on
        private IntPtr rewardedPtr = IntPtr.Zero;
        // This is passed to the Obj-C wrapper class so it can reference the Unity object in the callbacks
        private IntPtr unityRewardedPtr = IntPtr.Zero;
        private bool canPlay = false;

        internal delegate void VungleRewardedAdLoadedCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdFailedToLoadCallback(IntPtr rewardedAd, string error);
        internal delegate void VungleRewardedAdWillPresentCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdDidPresentCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdFailedToPresentCallback(IntPtr rewardedAd, string error);
        internal delegate void VungleRewardedAdWillCloseCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdDidCloseCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdDidTrackImpressionCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdDidClickCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdWillLeaveApplicationCallback(IntPtr rewardedAd);
        internal delegate void VungleRewardedAdDidRewardUserCallback(IntPtr rewardedAd);


        public VungleRewarded(string placementId)
        {
            this.placementId = placementId;
            this.BuildRewarded();   
        }

        public void BuildRewarded()
        {
            this.unityRewardedPtr = (IntPtr)GCHandle.Alloc(this);
            this.rewardedPtr = CreateVungleRewardedAd(this.unityRewardedPtr, this.placementId);
            SetVungleRewardedAdCallbacks(this.rewardedPtr,
               RewardedLoadedCallback,
               RewardedFailedToLoadCallback,
               RewardedWillPresentCallback,
               RewardedDidPresentCallback,
               RewardedFailedToPresentCallback,
               RewardedWillCloseCallback,
               RewardedDidCloseCallback,
               RewardedDidTrackImpressionCallback,
               RewardedDidClickCallback,
               RewardedWillLeaveApplicationCallback,
               RewardedDidRewardUserCallback);
        }

        ~VungleRewarded()
        {
            rewardedPtr = IntPtr.Zero;
            if (unityRewardedPtr != IntPtr.Zero)
            {
                ((GCHandle)unityRewardedPtr).Free();
                unityRewardedPtr = IntPtr.Zero;
            }
        }

        public bool CanPlay()
        {
            return canPlay && rewardedPtr != IntPtr.Zero;
        }

        public void Load()
        {
            if (rewardedPtr == IntPtr.Zero) return;
            LoadVungleRewardedAd(this.rewardedPtr);
        }

        public void LoadWithCsbData(VungleCSBData csbData)
        {
            if (rewardedPtr == IntPtr.Zero || csbData == null) return;
            int count = csbData.Extras != null ? csbData.Extras.Count : 0;
            string[] keys = count > 0 ? new string[count] : null;
            string[] values = count > 0 ? new string[count] : null;
            if (count > 0)
            {
                int i = 0;
                foreach (var kvp in csbData.Extras) { keys[i] = kvp.Key; values[i] = kvp.Value; i++; }
            }
            LoadVungleRewardedAdWithCsb(this.rewardedPtr, csbData.BidFloor, csbData.AuctionId, csbData.CreativeId, csbData.AdUnitId, csbData.IsVxWinner, csbData.IsPriorityAccess, keys, values, count);
        }

        public double GetWinningPrice()
        {
            if (rewardedPtr == IntPtr.Zero) return 0;
            return GetVungleRewardedAdWinningPrice(this.rewardedPtr);
        }

        public void SendWinURL()
        {
            if (rewardedPtr == IntPtr.Zero) return;
            SendVungleRewardedAdWinURL(this.rewardedPtr);
        }

        public void SendLossURL()
        {
            if (rewardedPtr == IntPtr.Zero) return;
            SendVungleRewardedAdLossURL(this.rewardedPtr);
        }

        public void Show()
        {
            if (rewardedPtr == IntPtr.Zero) return;
            canPlay = false;
            ShowVungleRewardedAd(this.rewardedPtr);
        }

        #region Rewarded Callbacks

        [MonoPInvokeCallback(typeof(VungleRewardedAdLoadedCallback))]
        private static void RewardedLoadedCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.canPlay = true;
            client.onLoadSuccess?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdFailedToLoadCallback))]
        private static void RewardedFailedToLoadCallback(IntPtr rewardedAd, string error)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onLoadFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdWillPresentCallback))]
        private static void RewardedWillPresentCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onWillPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdDidPresentCallback))]
        private static void RewardedDidPresentCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onDidPresent?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdFailedToPresentCallback))]
        private static void RewardedFailedToPresentCallback(IntPtr rewardedAd, string error)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onPresentFailed?.Invoke(error);
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdWillCloseCallback))]
        private static void RewardedWillCloseCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onWillClose?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdDidCloseCallback))]
        private static void RewardedDidCloseCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onDidClose?.Invoke();
            client.rewardedPtr = IntPtr.Zero;
            if (client.unityRewardedPtr != IntPtr.Zero)
            {
                ((GCHandle)client.unityRewardedPtr).Free();
                client.unityRewardedPtr = IntPtr.Zero;
            }
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdDidTrackImpressionCallback))]
        private static void RewardedDidTrackImpressionCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onImpression?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdDidClickCallback))]
        private static void RewardedDidClickCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onClick?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdWillLeaveApplicationCallback))]
        private static void RewardedWillLeaveApplicationCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onWillLeaveApplication?.Invoke();
        }

        [MonoPInvokeCallback(typeof(VungleRewardedAdDidRewardUserCallback))]
        private static void RewardedDidRewardUserCallback(IntPtr rewardedAd)
        {
            VungleRewarded client = IntPtrToRewarded(rewardedAd);
            if (client == null) return;
            client.onDidRewardUser?.Invoke();
        }

        private static VungleRewarded IntPtrToRewarded(IntPtr rewardedAd)
        {
            GCHandle handle = (GCHandle)rewardedAd;
            return handle.Target as VungleRewarded;
        }

        #endregion

        #region Rewarded Externs

        [DllImport("__Internal")]
        internal static extern IntPtr CreateVungleRewardedAd(IntPtr unityRef, string placementId);

        [DllImport("__Internal")]
        internal static extern void SetVungleRewardedAdCallbacks(IntPtr interstitialAd,
            VungleRewardedAdLoadedCallback adLoadedCallback,
            VungleRewardedAdFailedToLoadCallback adFailedToLoadCallback,
            VungleRewardedAdWillPresentCallback adWillPresentCallback,
            VungleRewardedAdDidPresentCallback adDidPresentCallback,
            VungleRewardedAdFailedToPresentCallback adFailedToPresentCallback,
            VungleRewardedAdWillCloseCallback adWillCloseCallback,
            VungleRewardedAdDidCloseCallback adDidCloseCallback,
            VungleRewardedAdDidTrackImpressionCallback adDidTrackImpressionCallback,
            VungleRewardedAdDidClickCallback adDidClickCallback,
            VungleRewardedAdWillLeaveApplicationCallback adWillLeaveApplicationCallback,
            VungleRewardedAdDidRewardUserCallback adDidRewardUserCallback);

        [DllImport("__Internal")]
        internal static extern void LoadVungleRewardedAd(IntPtr rewardedAd);

        [DllImport("__Internal")]
        internal static extern void LoadVungleRewardedAdWithCsb(IntPtr rewardedAd, double bidFloor, string auctionId, string creativeId, string adUnitId, bool isVxWinner, bool isPriorityAccess, string[] extrasKeys, string[] extrasValues, int extrasCount);

        [DllImport("__Internal")]
        internal static extern void ShowVungleRewardedAd(IntPtr rewardedAd);

        [DllImport("__Internal")]
        internal static extern double GetVungleRewardedAdWinningPrice(IntPtr rewardedAd);

        [DllImport("__Internal")]
        internal static extern void SendVungleRewardedAdWinURL(IntPtr rewardedAd);

        [DllImport("__Internal")]
        internal static extern void SendVungleRewardedAdLossURL(IntPtr rewardedAd);

        #endregion
    }

    #endif
}
