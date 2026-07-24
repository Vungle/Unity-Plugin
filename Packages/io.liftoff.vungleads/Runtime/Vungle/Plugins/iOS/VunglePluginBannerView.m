//
//  VunglePluginBannerView.m
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "VunglePluginBannerView.h"
@implementation VunglePluginBannerView

- (nonnull instancetype)initWithPlacementId:(NSString *)placementId adSizeType:(int)adSizeType width:(int)width height:(int)height unityRef:(VunglePluginUnityBannerViewRef)unityRef {
    self = [super init];
    if (self) {
        VungleAdSize *adSize;
        switch (adSizeType) {
            case 1:
                adSize = VungleAdSize.VungleAdSizeBannerShort;
                break;
            case 2:
                adSize = VungleAdSize.VungleAdSizeLeaderboard;
                break;
            case 3:
                adSize = VungleAdSize.VungleAdSizeMREC;
                break;
            case 4:
                adSize = [VungleAdSize VungleAdSizeWithWidth:(CGFloat)width];
                break;
            case 5:
                adSize = [VungleAdSize VungleAdSizeFromCGSize:CGSizeMake((CGFloat)width, (CGFloat)height)];
                break;
            default:
                adSize = VungleAdSize.VungleAdSizeBannerRegular;
                break;
        }
        self.bannerView = [[VungleBannerView alloc] initWithPlacementId:placementId vungleAdSize:adSize];
        self.bannerView.delegate = self;
        self.unityReference = unityRef;
        self.isLoaded = NO;
    }
    return self;
}

- (void)load {
    [self.bannerView load:nil];
}

- (void)attachAtX:(int)x y:(int)y width:(int)width height:(int)height {
    UIViewController *vc = UnityGetGLViewController();
    UIView *parentView = vc.view;

    CGFloat scale = [UIScreen mainScreen].scale;
    CGFloat frameX = x / scale;
    CGFloat frameY = y / scale;
    CGSize bannerSize = [self.bannerView getBannerSize];
    CGFloat frameW = width > 0 ? width / scale : bannerSize.width;
    CGFloat frameH = height > 0 ? height / scale : bannerSize.height;

    if (self.bannerView.superview != parentView) {
        [self.bannerView removeFromSuperview];
        [parentView addSubview:self.bannerView];
    }
    // Set frame after addSubview so any Auto Layout pass triggered by addSubview
    // doesn't overwrite our position.
    self.bannerView.frame = CGRectMake(frameX, frameY, frameW, frameH);
}

- (void)detach {
    [self.bannerView removeFromSuperview];
}

- (void)destroy {
    [self.bannerView removeFromSuperview];
    self.bannerView = nil;
}

#pragma mark - VungleBannerViewDelegate

- (void)bannerAdDidLoad:(VungleBannerView *)bannerView {
    self.isLoaded = YES;
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adLoadedCallback) {
            self.adLoadedCallback(self.unityReference);
        }
    });
}

- (void)bannerAdDidFail:(VungleBannerView *)bannerView withError:(NSError *)withError {
    BOOL wasLoaded = self.isLoaded;
    NSString *errorDescription = [withError localizedDescription];
    dispatch_async(dispatch_get_main_queue(), ^{
        if (!wasLoaded) {
            if (self.adFailedToLoadCallback) {
                self.adFailedToLoadCallback(self.unityReference, [errorDescription UTF8String]);
            }
        } else {
            if (self.adFailedToPresentCallback) {
                self.adFailedToPresentCallback(self.unityReference, [errorDescription UTF8String]);
            }
        }
    });
}

- (void)bannerAdWillPresent:(VungleBannerView *)bannerView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adWillPresentCallback) {
            self.adWillPresentCallback(self.unityReference);
        }
    });
}

- (void)bannerAdDidPresent:(VungleBannerView *)bannerView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adDidPresentCallback) {
            self.adDidPresentCallback(self.unityReference);
        }
    });
}

- (void)bannerAdWillClose:(VungleBannerView *)bannerView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adWillCloseCallback) {
            self.adWillCloseCallback(self.unityReference);
        }
    });
}

- (void)bannerAdDidClose:(VungleBannerView *)bannerView {
    self.isLoaded = NO;
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adDidCloseCallback) {
            self.adDidCloseCallback(self.unityReference);
        }
    });
}

- (void)bannerAdDidTrackImpression:(VungleBannerView *)bannerView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adDidTrackImpressionCallback) {
            self.adDidTrackImpressionCallback(self.unityReference);
        }
    });
}

- (void)bannerAdDidClick:(VungleBannerView *)bannerView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adDidClickCallback) {
            self.adDidClickCallback(self.unityReference);
        }
    });
}

- (void)bannerAdWillLeaveApplication:(VungleBannerView *)bannerView {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.adWillLeaveApplicationCallback) {
            self.adWillLeaveApplicationCallback(self.unityReference);
        }
    });
}

@end
