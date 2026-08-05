//
//  VunglePluginNativeAd.m
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "VunglePluginNativeAd.h"
#import "VunglePluginReferences.h"

extern UIViewController* UnityGetGLViewController(void);

// Container that swallows touches instead of letting them reach Unity.
// Plain UIViews forward unhandled touches up the responder chain into
// Unity's view, and Unity's engine normalizes each touch position by the
// bounds of the view the touch ORIGINATED on — container-local coordinates
// get stretched across the whole Unity screen, firing phantom clicks on
// unrelated Unity UI (e.g. a tap near the container's top edge lands on a
// button at the top of the screen). Overriding the touch methods as no-ops
// stops that forwarding for the container and every descendant, while
// hit-testing and the SDK's tap gesture recognizers keep working.
@interface VungleTouchBlockingView : UIView
@end

@implementation VungleTouchBlockingView
- (void)touchesBegan:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event {}
- (void)touchesMoved:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event {}
- (void)touchesEnded:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event {}
- (void)touchesCancelled:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event {}
@end

@interface VunglePluginNativeAd ()

@property VungleNative *nativeAd;
@property(nonatomic, strong) UIView *containerView;
@property(nonatomic, strong) MediaView *mediaView;
@property(nonatomic, strong) NSMutableArray<UIView *> *clickableOverlays;

@end

@implementation VunglePluginNativeAd

- (nonnull instancetype)initWithPlacementId:(NSString *)placementId unityRef:(VunglePluginUnityNativeAdRef)unityRef {
    self = [super init];
    if (self) {
        self.nativeAd = [[VungleNative alloc] initWithPlacementId:placementId];
        self.nativeAd.delegate = self;
        self.unityReference = unityRef;
    }
    return self;
}

- (void)load {
    [self.nativeAd load:nil];
}

- (void)attachAtX:(int)x y:(int)y width:(int)width height:(int)height {
    [self attachAtX:x y:y width:width height:height
             mediaX:x mediaY:y mediaWidth:width mediaHeight:height
     clickableRects:NULL clickableCount:0];
}

// clickableRects: flattened screen-coordinate rects (x, y, w, h per entry).
// A transparent overlay view is placed in the container for each rect and
// registered as clickable with the SDK; the media view is always clickable.
- (void)attachAtX:(int)x y:(int)y width:(int)width height:(int)height
           mediaX:(int)mediaX mediaY:(int)mediaY
       mediaWidth:(int)mediaWidth mediaHeight:(int)mediaHeight
   clickableRects:(const int *)clickableRects
   clickableCount:(int)clickableCount {
    UIViewController *vc = UnityGetGLViewController();
    UIView *parentView = vc.view;

    CGFloat scale = [UIScreen mainScreen].scale;
    CGFloat frameX = x / scale;
    CGFloat frameY = y / scale;
    CGFloat frameW = width / scale;
    CGFloat frameH = height / scale;
    CGFloat mediaFrameX = (mediaX - x) / scale;
    CGFloat mediaFrameY = (mediaY - y) / scale;
    CGFloat mediaFrameW = mediaWidth / scale;
    CGFloat mediaFrameH = mediaHeight / scale;

    BOOL firstAttach = (self.containerView == nil);
    if (firstAttach) {
        self.containerView = [[VungleTouchBlockingView alloc] init];
        self.containerView.clipsToBounds = YES;
        self.mediaView = [[MediaView alloc] init];
        [self.containerView addSubview:self.mediaView];
        self.clickableOverlays = [NSMutableArray array];
    }

    self.containerView.frame = CGRectMake(frameX, frameY, frameW, frameH);

    // Aspect-fit the media view within the media rect (container-relative)
    CGFloat ratio = [self.nativeAd getMediaAspectRatio];
    CGFloat mw = mediaFrameW, mh = mediaFrameH;
    if (ratio > 0) {
        if (mediaFrameW / mediaFrameH > ratio) {
            mh = mediaFrameH;
            mw = mediaFrameH * ratio;
        } else {
            mw = mediaFrameW;
            mh = mediaFrameW / ratio;
        }
    }
    self.mediaView.frame = CGRectMake(mediaFrameX + (mediaFrameW - mw) / 2,
                                      mediaFrameY + (mediaFrameH - mh) / 2,
                                      mw, mh);

    // Rebuild the clickable overlays if the set size changed
    BOOL rebuild = firstAttach || (NSUInteger)clickableCount != self.clickableOverlays.count;
    if (rebuild) {
        for (UIView *overlay in self.clickableOverlays) {
            [overlay removeFromSuperview];
        }
        [self.clickableOverlays removeAllObjects];
        for (int i = 0; i < clickableCount; i++) {
            UIView *overlay = [[UIView alloc] init];
            overlay.backgroundColor = [UIColor clearColor];
            [self.containerView addSubview:overlay];
            [self.clickableOverlays addObject:overlay];
        }
    }
    for (int i = 0; i < clickableCount; i++) {
        self.clickableOverlays[i].frame = CGRectMake((clickableRects[i * 4] - x) / scale,
                                                     (clickableRects[i * 4 + 1] - y) / scale,
                                                     clickableRects[i * 4 + 2] / scale,
                                                     clickableRects[i * 4 + 3] / scale);
    }

    if (self.containerView.superview != parentView) {
        [self.containerView removeFromSuperview];
        [parentView addSubview:self.containerView];
    }

    // Register after the view is in the hierarchy; re-register when the
    // clickable set changes. No explicit unregister is needed: the SDK's
    // registerViewForInteraction unregisters internally first (synchronously,
    // on the main thread), resets the ad to READY, and re-presents — and the
    // iOS delegate has no close callback, so re-registration is silent.
    if (rebuild) {
        [self.nativeAd registerViewForInteractionWithView:self.containerView
                                               mediaView:self.mediaView
                                           iconImageView:nil
                                          viewController:vc
                                          clickableViews:(self.clickableOverlays.count > 0 ? [self.clickableOverlays copy] : nil)];
    }
}

// Hides the ad but keeps the views and SDK registration so a later attach
// can show it again cheaply. Use destroy to release the ad entirely.
- (void)detach {
    [self.containerView removeFromSuperview];
}

// Full teardown: unregisters from the SDK (releasing the rendered media
// content, ad options view, and click gestures), removes the views, and
// detaches the delegate so no further callbacks reach Unity. Safe to call
// from any thread; view work is dispatched to the main queue.
- (void)destroy {
    self.nativeAd.delegate = nil;
    dispatch_async(dispatch_get_main_queue(), ^{
        [self.nativeAd unregisterView];
        [self.containerView removeFromSuperview];
        self.containerView = nil;
        self.mediaView = nil;
        self.clickableOverlays = nil;
        self.nativeAd = nil;
    });
}

#pragma mark - VungleNativeDelegate

- (void)nativeAdDidLoad:(nonnull VungleNative *)native {
    if (self.adLoadedCallback) {
        self.adLoadedCallback(self.unityReference);
    }
    if (self.adDataCallback) {
        NSString *title = native.title ?: @"";
        NSString *body = native.bodyText ?: @"";
        NSString *cta = native.callToAction ?: @"";
        double rating = native.adStarRating;
        NSString *iconUrl = @"";
        UIImage *iconImg = native.iconImage;
        if (iconImg) {
            NSString *tempPath = [NSTemporaryDirectory() stringByAppendingPathComponent:@"vungle_native_icon.png"];
            NSData *pngData = UIImagePNGRepresentation(iconImg);
            if (pngData && [pngData writeToFile:tempPath atomically:YES]) {
                iconUrl = tempPath;
            }
        }
        self.adDataCallback(self.unityReference,
                            [title UTF8String],
                            [body UTF8String],
                            [cta UTF8String],
                            rating,
                            [iconUrl UTF8String]);
    }
}

- (void)nativeAdDidFailToLoad:(nonnull VungleNative *)native withError:(nonnull NSError *)withError {
    if (self.adFailedToLoadCallback) {
        self.adFailedToLoadCallback(self.unityReference, [[withError localizedDescription] UTF8String]);
    }
}

- (void)nativeAdDidPresent:(VungleNative *)native {
    if (self.adDidPresentCallback) {
        self.adDidPresentCallback(self.unityReference);
    }
}

- (void)nativeAdDidFailToPresent:(VungleNative *)native withError:(NSError *)withError {
    // Registration failed (not loaded, expired, ...) — drop the half-built
    // views so the next attach rebuilds and re-registers from scratch instead
    // of assuming the SDK is wired to them
    [self.containerView removeFromSuperview];
    self.containerView = nil;
    self.mediaView = nil;
    self.clickableOverlays = nil;
    if (self.adFailedToPresentCallback) {
        self.adFailedToPresentCallback(self.unityReference, [[withError localizedDescription] UTF8String]);
    }
}

- (void)nativeAdDidClose:(VungleNative *)native {
    if (self.adDidCloseCallback) {
        self.adDidCloseCallback(self.unityReference);
    }
    [self.containerView removeFromSuperview];
    self.containerView = nil;
    self.mediaView = nil;
    self.clickableOverlays = nil;
    self.nativeAd.delegate = nil;
    self.nativeAd = nil;
    [VunglePluginReferences.sharedInstance removeObjectForKey:self];
}

- (void)nativeAdDidTrackImpression:(VungleNative *)native {
    if (self.adDidTrackImpressionCallback) {
        self.adDidTrackImpressionCallback(self.unityReference);
    }
}

- (void)nativeAdDidClick:(VungleNative *)native {
    if (self.adDidClickCallback) {
        self.adDidClickCallback(self.unityReference);
    }
}

- (void)nativeAdWillLeaveApplication:(VungleNative *)native {
    if (self.adWillLeaveApplicationCallback) {
        self.adWillLeaveApplicationCallback(self.unityReference);
    }
}

@end
