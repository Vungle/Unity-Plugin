//
//  VunglePluginSDK.m
//  UnityFramework
//

#import <Foundation/Foundation.h>
#import <VungleAdsSDK/VungleAdsSDK-Swift.h>
#import "VunglePluginSDK.h"

@implementation VunglePluginSDK

+ (void)initWithAppId:(NSString *)appId unityRef:(VunglePluginUnitySdkRef)unitySdkRef successCallback:(VungleSdkInitializeCompleteCallback)successCallback failureCallback:(VungleSdkFailedToInitializeCallback)failureCallback {
    NSLog(@"[VunglePlugin] initWithAppId: %@", appId);
    [VungleAds initWithAppId:appId completion:^(NSError * _Nullable error) {
        NSLog(@"[VunglePlugin] init completion fired, error: %@", error);
        void (^invokeCallbacks)(void) = ^{
            if (error != nil) {
                NSLog(@"[VunglePlugin] calling failure callback: %@", [error localizedDescription]);
                failureCallback(unitySdkRef, [[error localizedDescription] UTF8String]);
            } else {
                NSLog(@"[VunglePlugin] calling success callback");
                successCallback(unitySdkRef);
            }
        };
        if ([NSThread isMainThread]) {
            invokeCallbacks();
        } else {
            dispatch_async(dispatch_get_main_queue(), invokeCallbacks);
        }
    }];
}

@end
