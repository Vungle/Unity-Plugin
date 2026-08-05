namespace VungleAds.Samples
{
    public static class VungleConstants
    {
    #if UNITY_IOS
        public const string AppId = "604fabfd96303241d5b59873";
        public const string InterstitialPlacementId = "INTERSTITIAL_MRAID_1-8554253";
        public const string RewardedPlacementId = "REWARD_MRAID_1-5318383";
        public const string BannerPlacementId = "BANNER_1-5061683";
        public const string MrecPlacementId = "MREC_1-9433617";
        public const string InlinePlacementId = "IOS_INLINE_FLATCPM-7590046";
        public const string NativePlacementId = "NATIVEAD_1-7539365";
    #elif UNITY_ANDROID
        public const string AppId = "6285f650556a4f983b636be8";
        public const string InterstitialPlacementId = "NON_HB_INTERSTITIAL-1008518";
        public const string RewardedPlacementId = "NON_HB_REWARDED-5279128";
        public const string BannerPlacementId = "NON_HB_BANNER-2346628";
        public const string MrecPlacementId = "NON_HB_MREC-4656953";
        public const string InlinePlacementId = "ANDROID_INLINE_FLATCPM-1675834";
        public const string NativePlacementId = "NON_HB_NATIVE-4415663";
    #else
        public const string AppId = "";
        public const string InterstitialPlacementId = "";
        public const string RewardedPlacementId = "";
        public const string BannerPlacementId = "";
        public const string MrecPlacementId = "";
        public const string InlinePlacementId = "";
        public const string NativePlacementId = "";
    #endif
    }
}
