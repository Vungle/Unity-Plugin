//
//  VungleExterns.m
//  UnityFramework
//

#import <Foundation/Foundation.h>
#import "VunglePluginExterns.h"
#import "VunglePluginInterstitialAd.h"
#import "VunglePluginReferences.h"
#import "VunglePluginRewardedAd.h"
#import "VunglePluginBannerView.h"
#import "VunglePluginNativeAd.h"
#import "VunglePluginSDK.h"

#define GetStringParam(_x_) (_x_ != NULL) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]
#define GetStringParamOrNil(_x_) (_x_ != NULL) ? [NSString stringWithUTF8String:_x_] : nil

static NSArray<NSString *> *ExtrasArrayFromCStrings(const char * const *arr, int count) {
    if (!arr || count <= 0) return nil;
    NSMutableArray *result = [NSMutableArray arrayWithCapacity:count];
    for (int i = 0; i < count; i++) {
        [result addObject:arr[i] ? [NSString stringWithUTF8String:arr[i]] : @""];
    }
    return result;
}

#pragma mark - SDK calls

void InitializeVungleSdk(VunglePluginUnitySdkRef unitySdkRef,
                         const char * appId,
                         const char * pluginVersion,
                         VungleSdkInitializeCompleteCallback initializeCallback,
                         VungleSdkFailedToInitializeCallback failedToInitializeCallback) {
    [VunglePluginSDK initWithAppId:GetStringParam(appId)
                          unityRef:unitySdkRef
                   successCallback:initializeCallback
                   failureCallback:failedToInitializeCallback];
}

#pragma mark - Interstitial

VunglePluginInterstitialAdRef CreateVungleInterstitialAd(VunglePluginUnityInterstitialAdRef unityRef, const char *placementId) {
    VunglePluginInterstitialAd *pluginAd = [[VunglePluginInterstitialAd alloc] initWithPlacementId:GetStringParam(placementId) unityRef:unityRef];
    [VunglePluginReferences.sharedInstance setObject:pluginAd];
    return (__bridge VunglePluginInterstitialAdRef)pluginAd;
}

void SetVungleInterstitialAdCallbacks(VunglePluginInterstitialAdRef interstitialPluginRef,
                                      VungleInterstitialAdLoadedCallback adLoadedCallback,
                                      VungleInterstitialAdFailedToLoadCallback adFailedToLoadCallback,
                                      VungleInterstitialAdWillPresentCallback adWillPresentCallback,
                                      VungleInterstitialAdDidPresentCallback adDidPresentCallback,
                                      VungleInterstitialAdFailedToPresentCallback adFailedToPresentCallback,
                                      VungleInterstitialAdWillCloseCallback adWillCloseCallback,
                                      VungleInterstitialAdDidCloseCallback adDidCloseCallback,
                                      VungleInterstitialAdDidTrackImpressionCallback adDidTrackImpressionCallback,
                                      VungleInterstitialAdDidClickCallback adDidClickCallback,
                                      VungleInterstitialAdWillLeaveApplicationCallback adWillLeaveApplicationCallback) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    pluginAd.adLoadedCallback = adLoadedCallback;
    pluginAd.adFailedToLoadCallback = adFailedToLoadCallback;
    pluginAd.adWillPresentCallback = adWillPresentCallback;
    pluginAd.adDidPresentCallback = adDidPresentCallback;
    pluginAd.adFailedToPresentCallback = adFailedToPresentCallback;
    pluginAd.adWillCloseCallback = adWillCloseCallback;
    pluginAd.adDidCloseCallback = adDidCloseCallback;
    pluginAd.adDidTrackImpressionCallback = adDidTrackImpressionCallback;
    pluginAd.adDidClickCallback = adDidClickCallback;
    pluginAd.adWillLeaveApplicationCallback = adWillLeaveApplicationCallback;
}

void LoadVungleInterstitialAd(VunglePluginInterstitialAdRef interstitialPluginRef) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    [pluginAd load];
}

void LoadVungleInterstitialAdWithCsb(VunglePluginInterstitialAdRef interstitialPluginRef, double bidFloor, const char *auctionId, const char *creativeId, const char *adUnitId, BOOL isVxWinner, BOOL isPriorityAccess, const char * const *extrasKeys, const char * const *extrasValues, int extrasCount) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    VungleCSBData *csbData = [[VungleCSBData alloc] initWithBidFloor:bidFloor auctionId:GetStringParamOrNil(auctionId) creativeId:GetStringParamOrNil(creativeId) adUnitId:GetStringParamOrNil(adUnitId) isVxWinner:isVxWinner isPriorityAccess:isPriorityAccess];
    NSArray<NSString *> *keysArr = ExtrasArrayFromCStrings(extrasKeys, extrasCount);
    NSArray<NSString *> *valsArr = ExtrasArrayFromCStrings(extrasValues, extrasCount);
    if (keysArr && valsArr && keysArr.count == valsArr.count) {
        NSMutableDictionary<NSString *, NSString *> *extras = [NSMutableDictionary dictionary];
        for (NSUInteger i = 0; i < keysArr.count; i++) {
            extras[keysArr[i]] = valsArr[i];
        }
        [csbData setWithExtras:extras];
    }
    [pluginAd loadWithCsbData:csbData];
}

double GetVungleInterstitialAdWinningPrice(VunglePluginInterstitialAdRef interstitialPluginRef) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    return [pluginAd getWinningPrice];
}

void SendVungleInterstitialAdWinURL(VunglePluginInterstitialAdRef interstitialPluginRef) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    [pluginAd sendWinURL];
}

void SendVungleInterstitialAdLossURL(VunglePluginInterstitialAdRef interstitialPluginRef) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    [pluginAd sendLossURL];
}

void ShowVungleInterstitialAd(VunglePluginInterstitialAdRef interstitialPluginRef) {
    VunglePluginInterstitialAd *pluginAd = (__bridge VunglePluginInterstitialAd *)interstitialPluginRef;
    [pluginAd show];
}

#pragma mark - Rewarded

VunglePluginRewardedAdRef CreateVungleRewardedAd(VunglePluginUnityRewardedAdRef unityRef, const char *placementId) {
    VunglePluginRewardedAd *pluginAd = [[VunglePluginRewardedAd alloc] initWithPlacementId:GetStringParam(placementId) unityRef:unityRef];
    [VunglePluginReferences.sharedInstance setObject:pluginAd];
    return (__bridge VunglePluginRewardedAdRef)pluginAd;
}

void SetVungleRewardedAdCallbacks(VunglePluginRewardedAdRef interstitialPluginRef,
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
                                      VungleRewardedAdDidRewardUserCallback adDidRewardUserCallback) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)interstitialPluginRef;
    pluginAd.adLoadedCallback = adLoadedCallback;
    pluginAd.adFailedToLoadCallback = adFailedToLoadCallback;
    pluginAd.adWillPresentCallback = adWillPresentCallback;
    pluginAd.adDidPresentCallback = adDidPresentCallback;
    pluginAd.adFailedToPresentCallback = adFailedToPresentCallback;
    pluginAd.adWillCloseCallback = adWillCloseCallback;
    pluginAd.adDidCloseCallback = adDidCloseCallback;
    pluginAd.adDidTrackImpressionCallback = adDidTrackImpressionCallback;
    pluginAd.adDidClickCallback = adDidClickCallback;
    pluginAd.adWillLeaveApplicationCallback = adWillLeaveApplicationCallback;
    pluginAd.adDidRewardUserCallback = adDidRewardUserCallback;
}

void LoadVungleRewardedAd(VunglePluginRewardedAdRef interstitialPluginRef) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)interstitialPluginRef;
    [pluginAd load];
}

void LoadVungleRewardedAdWithCsb(VunglePluginRewardedAdRef rewardedPluginRef, double bidFloor, const char *auctionId, const char *creativeId, const char *adUnitId, BOOL isVxWinner, BOOL isPriorityAccess, const char * const *extrasKeys, const char * const *extrasValues, int extrasCount) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)rewardedPluginRef;
    VungleCSBData *csbData = [[VungleCSBData alloc] initWithBidFloor:bidFloor auctionId:GetStringParamOrNil(auctionId) creativeId:GetStringParamOrNil(creativeId) adUnitId:GetStringParamOrNil(adUnitId) isVxWinner:isVxWinner isPriorityAccess:isPriorityAccess];
    NSArray<NSString *> *keysArr = ExtrasArrayFromCStrings(extrasKeys, extrasCount);
    NSArray<NSString *> *valsArr = ExtrasArrayFromCStrings(extrasValues, extrasCount);
    if (keysArr && valsArr && keysArr.count == valsArr.count) {
        NSMutableDictionary<NSString *, NSString *> *extras = [NSMutableDictionary dictionary];
        for (NSUInteger i = 0; i < keysArr.count; i++) {
            extras[keysArr[i]] = valsArr[i];
        }
        [csbData setWithExtras:extras];
    }
    [pluginAd loadWithCsbData:csbData];
}

double GetVungleRewardedAdWinningPrice(VunglePluginRewardedAdRef rewardedPluginRef) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)rewardedPluginRef;
    return [pluginAd getWinningPrice];
}

void SendVungleRewardedAdWinURL(VunglePluginRewardedAdRef rewardedPluginRef) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)rewardedPluginRef;
    [pluginAd sendWinURL];
}

void SendVungleRewardedAdLossURL(VunglePluginRewardedAdRef rewardedPluginRef) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)rewardedPluginRef;
    [pluginAd sendLossURL];
}

void ShowVungleRewardedAd(VunglePluginRewardedAdRef interstitialPluginRef) {
    VunglePluginRewardedAd *pluginAd = (__bridge VunglePluginRewardedAd *)interstitialPluginRef;
    [pluginAd show];
}

#pragma mark - BannerView

VunglePluginBannerViewRef CreateVungleBannerView(VunglePluginUnityBannerViewRef unityRef, const char *placementId, int adSizeType, int width, int height) {
    VunglePluginBannerView *pluginAd = [[VunglePluginBannerView alloc] initWithPlacementId:GetStringParam(placementId) adSizeType:adSizeType width:width height:height unityRef:unityRef];
    [VunglePluginReferences.sharedInstance setObject:pluginAd];
    return (__bridge VunglePluginBannerViewRef)pluginAd;
}

void SetVungleBannerViewCallbacks(VunglePluginBannerViewRef bannerViewPluginRef,
                                  VungleBannerViewAdLoadedCallback adLoadedCallback,
                                  VungleBannerViewAdFailedToLoadCallback adFailedToLoadCallback,
                                  VungleBannerViewAdWillPresentCallback adWillPresentCallback,
                                  VungleBannerViewAdDidPresentCallback adDidPresentCallback,
                                  VungleBannerViewAdFailedToPresentCallback adFailedToPresentCallback,
                                  VungleBannerViewAdWillCloseCallback adWillCloseCallback,
                                  VungleBannerViewAdDidCloseCallback adDidCloseCallback,
                                  VungleBannerViewAdDidTrackImpressionCallback adDidTrackImpressionCallback,
                                  VungleBannerViewAdDidClickCallback adDidClickCallback,
                                  VungleBannerViewAdWillLeaveApplicationCallback adWillLeaveApplicationCallback) {
    VunglePluginBannerView *pluginAd = (__bridge VunglePluginBannerView *)bannerViewPluginRef;
    pluginAd.adLoadedCallback = adLoadedCallback;
    pluginAd.adFailedToLoadCallback = adFailedToLoadCallback;
    pluginAd.adWillPresentCallback = adWillPresentCallback;
    pluginAd.adDidPresentCallback = adDidPresentCallback;
    pluginAd.adFailedToPresentCallback = adFailedToPresentCallback;
    pluginAd.adWillCloseCallback = adWillCloseCallback;
    pluginAd.adDidCloseCallback = adDidCloseCallback;
    pluginAd.adDidTrackImpressionCallback = adDidTrackImpressionCallback;
    pluginAd.adDidClickCallback = adDidClickCallback;
    pluginAd.adWillLeaveApplicationCallback = adWillLeaveApplicationCallback;
}

void LoadVungleBannerView(VunglePluginBannerViewRef bannerViewPluginRef) {
    VunglePluginBannerView *pluginAd = (__bridge VunglePluginBannerView *)bannerViewPluginRef;
    [pluginAd load];
}

void AttachVungleBannerView(VunglePluginBannerViewRef bannerViewPluginRef, int x, int y, int width, int height) {
    VunglePluginBannerView *pluginAd = (__bridge VunglePluginBannerView *)bannerViewPluginRef;
    [pluginAd attachAtX:x y:y width:width height:height];
}

void DetachVungleBannerView(VunglePluginBannerViewRef bannerViewPluginRef) {
    VunglePluginBannerView *pluginAd = (__bridge VunglePluginBannerView *)bannerViewPluginRef;
    [pluginAd detach];
}

void DestroyVungleBannerView(VunglePluginBannerViewRef bannerViewPluginRef) {
    VunglePluginBannerView *pluginAd = (__bridge VunglePluginBannerView *)bannerViewPluginRef;
    [pluginAd destroy];
    [VunglePluginReferences.sharedInstance removeObjectForKey:(__bridge id)bannerViewPluginRef];
}

#pragma mark - Native

VunglePluginNativeAdRef CreateVungleNativeAd(VunglePluginUnityNativeAdRef unityRef, const char *placementId) {
    VunglePluginNativeAd *pluginAd = [[VunglePluginNativeAd alloc] initWithPlacementId:GetStringParam(placementId) unityRef:unityRef];
    [VunglePluginReferences.sharedInstance setObject:pluginAd];
    return (__bridge VunglePluginNativeAdRef)pluginAd;
}

void SetVungleNativeAdCallbacks(VunglePluginNativeAdRef nativePluginRef,
                                VungleNativeAdLoadedCallback adLoadedCallback,
                                VungleNativeAdFailedToLoadCallback adFailedToLoadCallback,
                                VungleNativeAdDidPresentCallback adDidPresentCallback,
                                VungleNativeAdFailedToPresentCallback adFailedToPresentCallback,
                                VungleNativeAdDidCloseCallback adDidCloseCallback,
                                VungleNativeAdDidTrackImpressionCallback adDidTrackImpressionCallback,
                                VungleNativeAdDidClickCallback adDidClickCallback,
                                VungleNativeAdWillLeaveApplicationCallback adWillLeaveApplicationCallback,
                                VungleNativeAdDataCallback adDataCallback) {
    VunglePluginNativeAd *pluginAd = (__bridge VunglePluginNativeAd *)nativePluginRef;
    pluginAd.adLoadedCallback = adLoadedCallback;
    pluginAd.adFailedToLoadCallback = adFailedToLoadCallback;
    pluginAd.adDidPresentCallback = adDidPresentCallback;
    pluginAd.adFailedToPresentCallback = adFailedToPresentCallback;
    pluginAd.adDidCloseCallback = adDidCloseCallback;
    pluginAd.adDidTrackImpressionCallback = adDidTrackImpressionCallback;
    pluginAd.adDidClickCallback = adDidClickCallback;
    pluginAd.adWillLeaveApplicationCallback = adWillLeaveApplicationCallback;
    pluginAd.adDataCallback = adDataCallback;
}

void LoadVungleNativeAd(VunglePluginNativeAdRef nativePluginRef) {
    VunglePluginNativeAd *pluginAd = (__bridge VunglePluginNativeAd *)nativePluginRef;
    [pluginAd load];
}

void AttachVungleNativeAd(VunglePluginNativeAdRef nativePluginRef, int x, int y, int width, int height) {
    VunglePluginNativeAd *pluginAd = (__bridge VunglePluginNativeAd *)nativePluginRef;
    [pluginAd attachAtX:x y:y width:width height:height];
}

void AttachVungleNativeAdEx(VunglePluginNativeAdRef nativePluginRef, int x, int y, int width, int height,
                            int mediaX, int mediaY, int mediaWidth, int mediaHeight,
                            const int *clickableRects, int clickableCount) {
    VunglePluginNativeAd *pluginAd = (__bridge VunglePluginNativeAd *)nativePluginRef;
    [pluginAd attachAtX:x y:y width:width height:height
                 mediaX:mediaX mediaY:mediaY mediaWidth:mediaWidth mediaHeight:mediaHeight
         clickableRects:clickableRects clickableCount:clickableCount];
}

void DetachVungleNativeAd(VunglePluginNativeAdRef nativePluginRef) {
    VunglePluginNativeAd *pluginAd = (__bridge VunglePluginNativeAd *)nativePluginRef;
    [pluginAd detach];
}

void DestroyVungleNativeAd(VunglePluginNativeAdRef nativePluginRef) {
    VunglePluginNativeAd *pluginAd = (__bridge VunglePluginNativeAd *)nativePluginRef;
    [pluginAd destroy];
    [VunglePluginReferences.sharedInstance removeObjectForKey:(__bridge id)nativePluginRef];
}
