//
//  VunglePluginInterstitialAd.m
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "VunglePluginInterstitialAd.h"
#import "VunglePluginReferences.h"
#import "VunglePluginUtil.h"

@interface VunglePluginInterstitialAd ()

@property VungleInterstitial *interstitialAd;

@end

@implementation VunglePluginInterstitialAd

- (nonnull instancetype)initWithPlacementId:(NSString *)placementId unityRef:(VunglePluginUnityInterstitialAdRef)unityRef {
    self = [super init];
    if (self) {
        self.interstitialAd = [[VungleInterstitial alloc] initWithPlacementId:placementId];
        self.interstitialAd.delegate = self;
        self.unityReference = unityRef;
    }
    return self;
}

- (void)load {
    [self.interstitialAd load:nil];
}

- (void)loadWithCsbData:(VungleCSBData *)csbData {
    [self.interstitialAd loadWithCSBData:csbData];
}

- (double)getWinningPrice {
    return [self.interstitialAd getWinningPrice];
}

- (void)sendWinURL {
    [self.interstitialAd sendWinURL];
}

- (void)sendLossURL {
    [self.interstitialAd sendLossURL];
}

- (void)show {
    [self.interstitialAd presentWith:UnityGetGLViewController()];
}

#pragma mark - VungleInterstitialDelegate

- (void)interstitialAdDidLoad:(nonnull VungleInterstitial *)interstitial {
    if (self.adLoadedCallback) {
        self.adLoadedCallback(self.unityReference);
    }
}

- (void)interstitialAdDidFailToLoad:(nonnull VungleInterstitial *)interstitial withError:(nonnull NSError *)withError {
    if (self.adFailedToLoadCallback) {
        self.adFailedToLoadCallback(self.unityReference, [[withError localizedDescription] UTF8String]);
    }
}

- (void)interstitialAdWillPresent:(VungleInterstitial *)interstitial {
    if (self.adWillPresentCallback) {
        self.adWillPresentCallback(self.unityReference);
    }
}

- (void)interstitialAdDidPresent:(VungleInterstitial *)interstitial {
    if (self.adDidPresentCallback) {
        self.adDidPresentCallback(self.unityReference);
    }
}

- (void)interstitialAdDidFailToPresent:(VungleInterstitial *)interstitial withError:(NSError *)withError {
    if (self.adFailedToPresentCallback) {
        self.adFailedToPresentCallback(self.unityReference, [[withError localizedDescription] UTF8String]);
    }
}

- (void)interstitialAdWillClose:(VungleInterstitial *)interstitial {
    if (self.adWillCloseCallback) {
        self.adWillCloseCallback(self.unityReference);
    }
}

- (void)interstitialAdDidClose:(VungleInterstitial *)interstitial {
    if (self.adDidCloseCallback) {
        self.adDidCloseCallback(self.unityReference);
    }
    self.interstitialAd.delegate = nil;
    self.interstitialAd = nil;
    [VunglePluginReferences.sharedInstance removeObjectForKey:self];
}

- (void)interstitialAdDidTrackImpression:(VungleInterstitial *)interstitial {
    if (self.adDidTrackImpressionCallback) {
        self.adDidTrackImpressionCallback(self.unityReference);
    }
}

- (void)interstitialAdDidClick:(VungleInterstitial *)interstitial {
    if (self.adDidClickCallback) {
        self.adDidClickCallback(self.unityReference);
    }
}

- (void)interstitialAdWillLeaveApplication:(VungleInterstitial *)interstitial {
    if (self.adWillLeaveApplicationCallback) {
        self.adWillLeaveApplicationCallback(self.unityReference);
    }
}

@end
