//
//  VunglePluginNativeAd.h
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <VungleAdsSDK/VungleAdsSDK-Swift.h>
#import "VunglePluginExterns.h"

@interface VunglePluginNativeAd : NSObject <VungleNativeDelegate>

@property(nonatomic, assign, nullable) VunglePluginUnityNativeAdRef unityReference;
@property(nonatomic, assign, nullable) VungleNativeAdLoadedCallback adLoadedCallback;
@property(nonatomic, assign, nullable) VungleNativeAdFailedToLoadCallback adFailedToLoadCallback;
@property(nonatomic, assign, nullable) VungleNativeAdDidPresentCallback adDidPresentCallback;
@property(nonatomic, assign, nullable) VungleNativeAdFailedToPresentCallback adFailedToPresentCallback;
@property(nonatomic, assign, nullable) VungleNativeAdDidCloseCallback adDidCloseCallback;
@property(nonatomic, assign, nullable) VungleNativeAdDidTrackImpressionCallback adDidTrackImpressionCallback;
@property(nonatomic, assign, nullable) VungleNativeAdDidClickCallback adDidClickCallback;
@property(nonatomic, assign, nullable) VungleNativeAdWillLeaveApplicationCallback adWillLeaveApplicationCallback;
@property(nonatomic, assign, nullable) VungleNativeAdDataCallback adDataCallback;

- (nonnull instancetype)init NS_UNAVAILABLE;
- (nonnull instancetype)initWithPlacementId:(NSString *_Nonnull)placementId unityRef:(VunglePluginUnityNativeAdRef _Nonnull)unityRef;
- (void)load;
- (void)attachAtX:(int)x y:(int)y width:(int)width height:(int)height;
- (void)attachAtX:(int)x y:(int)y width:(int)width height:(int)height
           mediaX:(int)mediaX mediaY:(int)mediaY
       mediaWidth:(int)mediaWidth mediaHeight:(int)mediaHeight
   clickableRects:(const int *_Nullable)clickableRects
   clickableCount:(int)clickableCount;
- (void)detach;
- (void)destroy;

@end
