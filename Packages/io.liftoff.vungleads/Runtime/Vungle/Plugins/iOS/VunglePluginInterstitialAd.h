//
//  VunglePluginInterstitialAd.h
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <VungleAdsSDK/VungleAdsSDK-Swift.h>
#import "VunglePluginExterns.h"

@interface VunglePluginInterstitialAd : NSObject <VungleInterstitialDelegate>

@property(nonatomic, assign, nullable) VunglePluginUnityInterstitialAdRef unityReference;
@property(nonatomic, assign, nullable) VungleInterstitialAdLoadedCallback adLoadedCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdFailedToLoadCallback adFailedToLoadCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdWillPresentCallback adWillPresentCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdDidPresentCallback adDidPresentCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdFailedToPresentCallback adFailedToPresentCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdWillCloseCallback adWillCloseCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdDidCloseCallback adDidCloseCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdDidTrackImpressionCallback adDidTrackImpressionCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdDidClickCallback adDidClickCallback;
@property(nonatomic, assign, nullable) VungleInterstitialAdWillLeaveApplicationCallback adWillLeaveApplicationCallback;

- (nonnull instancetype)init NS_UNAVAILABLE;
- (nonnull instancetype)initWithPlacementId:(NSString *_Nonnull)placementId unityRef:(VunglePluginUnityInterstitialAdRef _Nonnull )unityRef;
- (void)load;
- (void)loadWithCsbData:(VungleCSBData *_Nonnull)csbData;
- (void)show;
- (double)getWinningPrice;
- (void)sendWinURL;
- (void)sendLossURL;

@end
