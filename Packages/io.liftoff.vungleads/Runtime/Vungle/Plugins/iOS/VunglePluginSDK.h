//
//  VunglePluginSDK.h
//  UnityFramework
//

#import <Foundation/Foundation.h>
#import "VunglePluginExterns.h"

@interface VunglePluginSDK : NSObject

+ (void)initWithAppId:(NSString * _Nonnull)appId unityRef:(VunglePluginUnitySdkRef _Nonnull)unitySdkRef successCallback:(VungleSdkInitializeCompleteCallback _Nonnull)successCallback failureCallback:(VungleSdkFailedToInitializeCallback _Nonnull)failureCallback;

@end
