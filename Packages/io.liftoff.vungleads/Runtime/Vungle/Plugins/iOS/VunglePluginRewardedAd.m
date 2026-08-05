//
//  VunglePluginRewardedAd.m
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "VunglePluginRewardedAd.h"
#import "VunglePluginReferences.h"
#import "VunglePluginUtil.h"

@interface VunglePluginRewardedAd ()

@property VungleRewarded *rewardedAd;

@end

@implementation VunglePluginRewardedAd

- (nonnull instancetype)initWithPlacementId:(NSString *)placementId unityRef:(VunglePluginUnityRewardedAdRef)unityRef {
    self = [super init];
    if (self) {
        self.rewardedAd = [[VungleRewarded alloc] initWithPlacementId:placementId];
        self.rewardedAd.delegate = self;
        self.unityReference = unityRef;
    }
    return self;
}

- (void)load {
    [self.rewardedAd load:nil];
}

- (void)loadWithCsbData:(VungleCSBData *)csbData {
    [self.rewardedAd loadWithCSBData:csbData];
}

- (double)getWinningPrice {
    return [self.rewardedAd getWinningPrice];
}

- (void)sendWinURL {
    [self.rewardedAd sendWinURL];
}

- (void)sendLossURL {
    [self.rewardedAd sendLossURL];
}

- (void)show {
    [self.rewardedAd presentWith:UnityGetGLViewController()];
}

#pragma mark - VungleRewardedDelegate

- (void)rewardedAdDidLoad:(nonnull VungleRewarded *)rewarded {
    if (self.adLoadedCallback) {
        self.adLoadedCallback(self.unityReference);
    }
}

- (void)rewardedAdDidFailToLoad:(nonnull VungleRewarded *)rewarded withError:(nonnull NSError *)withError {
    if (self.adFailedToLoadCallback) {
        self.adFailedToLoadCallback(self.unityReference, [[withError localizedDescription] UTF8String]);
    }
}

- (void)rewardedAdWillPresent:(VungleRewarded *)rewarded {
    if (self.adWillPresentCallback) {
        self.adWillPresentCallback(self.unityReference);
    }
}

- (void)rewardedAdDidPresent:(VungleRewarded *)rewarded {
    if (self.adDidPresentCallback) {
        self.adDidPresentCallback(self.unityReference);
    }
}

- (void)rewardedAdDidFailToPresent:(VungleRewarded *)rewarded withError:(NSError *)withError {
    if (self.adFailedToPresentCallback) {
        self.adFailedToPresentCallback(self.unityReference, [[withError localizedDescription] UTF8String]);
    }
}

- (void)rewardedAdWillClose:(VungleRewarded *)rewarded {
    if (self.adWillCloseCallback) {
        self.adWillCloseCallback(self.unityReference);
    }
}

- (void)rewardedAdDidClose:(VungleRewarded *)rewarded {
    if (self.adDidCloseCallback) {
        self.adDidCloseCallback(self.unityReference);
    }
    self.rewardedAd.delegate = nil;
    self.rewardedAd = nil;
    [VunglePluginReferences.sharedInstance removeObjectForKey:self];
}

- (void)rewardedAdDidTrackImpression:(VungleRewarded *)rewarded {
    if (self.adDidTrackImpressionCallback) {
        self.adDidTrackImpressionCallback(self.unityReference);
    }
}

- (void)rewardedAdDidClick:(VungleRewarded *)rewarded {
    if (self.adDidClickCallback) {
        self.adDidClickCallback(self.unityReference);
    }
}

- (void)rewardedAdWillLeaveApplication:(VungleRewarded *)rewarded {
    if (self.adWillLeaveApplicationCallback) {
        self.adWillLeaveApplicationCallback(self.unityReference);
    }
}

- (void)rewardedAdDidRewardUser:(VungleRewarded *)rewarded {
    if (self.adDidRewardUserCallback) {
        self.adDidRewardUserCallback(self.unityReference);
    }
}

@end
