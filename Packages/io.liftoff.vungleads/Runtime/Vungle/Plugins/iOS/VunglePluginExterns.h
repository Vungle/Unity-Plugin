//
//  VungleExterns.h
//  Unity-iPhone
//

typedef const void *VunglePluginUnitySdkRef;

typedef void (*VungleSdkInitializeCompleteCallback)(VunglePluginUnitySdkRef _Nonnull unityRef);
typedef void (*VungleSdkFailedToInitializeCallback)(VunglePluginUnitySdkRef _Nonnull unityRef, const char * _Nonnull error);

#pragma mark - Interstitial Types

typedef const void *VunglePluginUnityInterstitialAdRef;
typedef const void *VunglePluginInterstitialAdRef;

typedef void (*VungleInterstitialAdLoadedCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdFailedToLoadCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleInterstitialAdWillPresentCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdDidPresentCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdFailedToPresentCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleInterstitialAdWillCloseCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdDidCloseCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdDidTrackImpressionCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdDidClickCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);
typedef void (*VungleInterstitialAdWillLeaveApplicationCallback)(VunglePluginUnityInterstitialAdRef _Nonnull unityRef);

#pragma mark - Rewarded Types

typedef const void *VunglePluginUnityRewardedAdRef;
typedef const void *VunglePluginRewardedAdRef;

typedef void (*VungleRewardedAdLoadedCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdFailedToLoadCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleRewardedAdWillPresentCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdDidPresentCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdFailedToPresentCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleRewardedAdWillCloseCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdDidCloseCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdDidTrackImpressionCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdDidClickCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdWillLeaveApplicationCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);
typedef void (*VungleRewardedAdDidRewardUserCallback)(VunglePluginUnityRewardedAdRef _Nonnull unityRef);

#pragma mark - SDK calls

void InitializeVungleSdk(VunglePluginUnitySdkRef _Nonnull sdkRef,
                         const char * _Nonnull appId,
                         const char * _Nonnull pluginVersion,
                         VungleSdkInitializeCompleteCallback _Nonnull initializeCallback,
                         VungleSdkFailedToInitializeCallback _Nonnull failedToInitializeCallback);

#pragma mark - Interstitial

VunglePluginInterstitialAdRef _Nonnull CreateVungleInterstitialAd(VunglePluginUnityInterstitialAdRef _Nonnull unityRef, const char * _Nonnull placementId);
void SetVungleInterstitialAdCallbacks(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef,
                                      VungleInterstitialAdLoadedCallback _Nonnull adLoadedCallback,
                                      VungleInterstitialAdFailedToLoadCallback _Nonnull adFailedToLoadCallback,
                                      VungleInterstitialAdWillPresentCallback _Nonnull adWillPresentCallback,
                                      VungleInterstitialAdDidPresentCallback _Nonnull adDidPresentCallback,
                                      VungleInterstitialAdFailedToPresentCallback _Nonnull adFailedToPresentCallback,
                                      VungleInterstitialAdWillCloseCallback _Nonnull adWillCloseCallback,
                                      VungleInterstitialAdDidCloseCallback _Nonnull adDidCloseCallback,
                                      VungleInterstitialAdDidTrackImpressionCallback _Nonnull adDidTrackImpressionCallback,
                                      VungleInterstitialAdDidClickCallback _Nonnull adDidClickCallback,
                                      VungleInterstitialAdWillLeaveApplicationCallback _Nonnull adWillLeaveApplicationCallback);
void LoadVungleInterstitialAd(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef);
void LoadVungleInterstitialAdWithCsb(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef, double bidFloor, const char * _Nullable auctionId, const char * _Nullable creativeId, const char * _Nullable adUnitId, BOOL isVxWinner, BOOL isPriorityAccess, const char * _Nullable const * _Nullable extrasKeys, const char * _Nullable const * _Nullable extrasValues, int extrasCount);
void ShowVungleInterstitialAd(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef);
double GetVungleInterstitialAdWinningPrice(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef);
void SendVungleInterstitialAdWinURL(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef);
void SendVungleInterstitialAdLossURL(VunglePluginInterstitialAdRef _Nonnull interstitialPluginRef);

#pragma mark - Rewarded

VunglePluginRewardedAdRef _Nonnull CreateVungleRewardedAd(VunglePluginUnityRewardedAdRef _Nonnull unityRef, const char * _Nonnull placementId);
void SetVungleRewardedAdCallbacks(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef,
                                      VungleRewardedAdLoadedCallback _Nonnull adLoadedCallback,
                                      VungleRewardedAdFailedToLoadCallback _Nonnull adFailedToLoadCallback,
                                      VungleRewardedAdWillPresentCallback _Nonnull adWillPresentCallback,
                                      VungleRewardedAdDidPresentCallback _Nonnull adDidPresentCallback,
                                      VungleRewardedAdFailedToPresentCallback _Nonnull adFailedToPresentCallback,
                                      VungleRewardedAdWillCloseCallback _Nonnull adWillCloseCallback,
                                      VungleRewardedAdDidCloseCallback _Nonnull adDidCloseCallback,
                                      VungleRewardedAdDidTrackImpressionCallback _Nonnull adDidTrackImpressionCallback,
                                      VungleRewardedAdDidClickCallback _Nonnull adDidClickCallback,
                                      VungleRewardedAdWillLeaveApplicationCallback _Nonnull adWillLeaveApplicationCallback,
                                      VungleRewardedAdDidRewardUserCallback _Nonnull adDidRewardUserCallback);
void LoadVungleRewardedAd(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef);
void LoadVungleRewardedAdWithCsb(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef, double bidFloor, const char * _Nullable auctionId, const char * _Nullable creativeId, const char * _Nullable adUnitId, BOOL isVxWinner, BOOL isPriorityAccess, const char * _Nullable const * _Nullable extrasKeys, const char * _Nullable const * _Nullable extrasValues, int extrasCount);
void ShowVungleRewardedAd(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef);
double GetVungleRewardedAdWinningPrice(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef);
void SendVungleRewardedAdWinURL(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef);
void SendVungleRewardedAdLossURL(VunglePluginRewardedAdRef _Nonnull rewardedPluginRef);

#pragma mark - BannerView Types

typedef const void *VunglePluginUnityBannerViewRef;
typedef const void *VunglePluginBannerViewRef;

typedef void (*VungleBannerViewAdLoadedCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdFailedToLoadCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleBannerViewAdWillPresentCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdDidPresentCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdFailedToPresentCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleBannerViewAdWillCloseCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdDidCloseCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdDidTrackImpressionCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdDidClickCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);
typedef void (*VungleBannerViewAdWillLeaveApplicationCallback)(VunglePluginUnityBannerViewRef _Nonnull unityRef);

#pragma mark - BannerView

VunglePluginBannerViewRef _Nonnull CreateVungleBannerView(VunglePluginUnityBannerViewRef _Nonnull unityRef, const char * _Nonnull placementId, int adSizeType, int width, int height);
void SetVungleBannerViewCallbacks(VunglePluginBannerViewRef _Nonnull bannerViewPluginRef,
                                  VungleBannerViewAdLoadedCallback _Nonnull adLoadedCallback,
                                  VungleBannerViewAdFailedToLoadCallback _Nonnull adFailedToLoadCallback,
                                  VungleBannerViewAdWillPresentCallback _Nonnull adWillPresentCallback,
                                  VungleBannerViewAdDidPresentCallback _Nonnull adDidPresentCallback,
                                  VungleBannerViewAdFailedToPresentCallback _Nonnull adFailedToPresentCallback,
                                  VungleBannerViewAdWillCloseCallback _Nonnull adWillCloseCallback,
                                  VungleBannerViewAdDidCloseCallback _Nonnull adDidCloseCallback,
                                  VungleBannerViewAdDidTrackImpressionCallback _Nonnull adDidTrackImpressionCallback,
                                  VungleBannerViewAdDidClickCallback _Nonnull adDidClickCallback,
                                  VungleBannerViewAdWillLeaveApplicationCallback _Nonnull adWillLeaveApplicationCallback);
void LoadVungleBannerView(VunglePluginBannerViewRef _Nonnull bannerViewPluginRef);
void AttachVungleBannerView(VunglePluginBannerViewRef _Nonnull bannerViewPluginRef, int x, int y, int width, int height);
void DetachVungleBannerView(VunglePluginBannerViewRef _Nonnull bannerViewPluginRef);
void DestroyVungleBannerView(VunglePluginBannerViewRef _Nonnull bannerViewPluginRef);

#pragma mark - Native Types

typedef const void *VunglePluginUnityNativeAdRef;
typedef const void *VunglePluginNativeAdRef;

typedef void (*VungleNativeAdLoadedCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef);
typedef void (*VungleNativeAdFailedToLoadCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleNativeAdDidPresentCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef);
typedef void (*VungleNativeAdFailedToPresentCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef, const char * _Nonnull error);
typedef void (*VungleNativeAdDidCloseCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef);
typedef void (*VungleNativeAdDidTrackImpressionCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef);
typedef void (*VungleNativeAdDidClickCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef);
typedef void (*VungleNativeAdWillLeaveApplicationCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef);
typedef void (*VungleNativeAdDataCallback)(VunglePluginUnityNativeAdRef _Nonnull unityRef, const char * _Nonnull title, const char * _Nonnull body, const char * _Nonnull ctaText, double rating, const char * _Nonnull iconUrl);

#pragma mark - Native

VunglePluginNativeAdRef _Nonnull CreateVungleNativeAd(VunglePluginUnityNativeAdRef _Nonnull unityRef, const char * _Nonnull placementId);
void SetVungleNativeAdCallbacks(VunglePluginNativeAdRef _Nonnull nativePluginRef,
                                VungleNativeAdLoadedCallback _Nonnull adLoadedCallback,
                                VungleNativeAdFailedToLoadCallback _Nonnull adFailedToLoadCallback,
                                VungleNativeAdDidPresentCallback _Nonnull adDidPresentCallback,
                                VungleNativeAdFailedToPresentCallback _Nonnull adFailedToPresentCallback,
                                VungleNativeAdDidCloseCallback _Nonnull adDidCloseCallback,
                                VungleNativeAdDidTrackImpressionCallback _Nonnull adDidTrackImpressionCallback,
                                VungleNativeAdDidClickCallback _Nonnull adDidClickCallback,
                                VungleNativeAdWillLeaveApplicationCallback _Nonnull adWillLeaveApplicationCallback,
                                VungleNativeAdDataCallback _Nonnull adDataCallback);
void LoadVungleNativeAd(VunglePluginNativeAdRef _Nonnull nativePluginRef);
void AttachVungleNativeAd(VunglePluginNativeAdRef _Nonnull nativePluginRef, int x, int y, int width, int height);
void DetachVungleNativeAd(VunglePluginNativeAdRef _Nonnull nativePluginRef);
