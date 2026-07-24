//
//  VunglePluginRewardedAd.h
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <VungleAdsSDK/VungleAdsSDK-Swift.h>
#import "VunglePluginExterns.h"

@interface VunglePluginRewardedAd : NSObject <VungleRewardedDelegate>

@property(nonatomic, assign, nullable) VunglePluginUnityRewardedAdRef unityReference;
@property(nonatomic, assign, nullable) VungleRewardedAdLoadedCallback adLoadedCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdFailedToLoadCallback adFailedToLoadCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdWillPresentCallback adWillPresentCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdDidPresentCallback adDidPresentCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdFailedToPresentCallback adFailedToPresentCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdWillCloseCallback adWillCloseCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdDidCloseCallback adDidCloseCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdDidTrackImpressionCallback adDidTrackImpressionCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdDidClickCallback adDidClickCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdWillLeaveApplicationCallback adWillLeaveApplicationCallback;
@property(nonatomic, assign, nullable) VungleRewardedAdDidRewardUserCallback adDidRewardUserCallback;

- (nonnull instancetype)init NS_UNAVAILABLE;
- (nonnull instancetype)initWithPlacementId:(NSString *_Nonnull)placementId unityRef:(VunglePluginUnityRewardedAdRef _Nonnull )unityRef;
- (void)load;
- (void)loadWithCsbData:(VungleCSBData *_Nonnull)csbData;
- (void)show;
- (double)getWinningPrice;
- (void)sendWinURL;
- (void)sendLossURL;

@end
