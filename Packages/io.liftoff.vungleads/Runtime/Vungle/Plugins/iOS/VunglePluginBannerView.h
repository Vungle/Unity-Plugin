//
//  VunglePluginBannerView.h
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <VungleAdsSDK/VungleAdsSDK-Swift.h>
#import "VunglePluginExterns.h"

@interface VunglePluginBannerView : NSObject <VungleBannerViewDelegate>

@property(nonatomic, assign, nullable) VunglePluginUnityBannerViewRef unityReference;
@property(nonatomic, strong, nullable) VungleBannerView *bannerView;
@property(nonatomic, assign) BOOL isLoaded;
@property(nonatomic, assign, nullable) VungleBannerViewAdLoadedCallback adLoadedCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdFailedToLoadCallback adFailedToLoadCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdWillPresentCallback adWillPresentCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdDidPresentCallback adDidPresentCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdFailedToPresentCallback adFailedToPresentCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdWillCloseCallback adWillCloseCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdDidCloseCallback adDidCloseCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdDidTrackImpressionCallback adDidTrackImpressionCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdDidClickCallback adDidClickCallback;
@property(nonatomic, assign, nullable) VungleBannerViewAdWillLeaveApplicationCallback adWillLeaveApplicationCallback;

- (nonnull instancetype)init NS_UNAVAILABLE;
- (nonnull instancetype)initWithPlacementId:(NSString *_Nonnull)placementId adSizeType:(int)adSizeType width:(int)width height:(int)height unityRef:(VunglePluginUnityBannerViewRef _Nonnull)unityRef;
- (void)load;
- (void)attachAtX:(int)x y:(int)y width:(int)width height:(int)height;
- (void)detach;
- (void)destroy;

@end
