using System;
using UnityEngine;

namespace VungleAds
{
    public interface IVungleRewarded
    {
        bool CanPlay();
        void Load();
        void LoadWithCsbData(VungleCSBData csbData);
        void Show();
        double GetWinningPrice();
        void SendWinURL();
        void SendLossURL();
    }

    public partial class VungleRewarded : IVungleRewarded
    {
        public string placementId { get; private set; }
        public Action onLoadSuccess { get; set; }
        public Action<string> onLoadFailed { get; set; }
        public Action onWillPresent { get; set; }
        public Action onDidPresent { get; set; }
        public Action<string> onPresentFailed { get; set; }
        public Action onWillClose { get; set; }
        public Action onDidClose { get; set; }
        public Action onImpression { get; set; }
        public Action onClick { get; set; }
        public Action onWillLeaveApplication { get; set; }
        public Action onDidRewardUser { get; set; }
    }
}
