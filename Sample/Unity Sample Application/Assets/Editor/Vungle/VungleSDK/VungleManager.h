//
//  VungleManager.h
//  VungleTest
//
//  Created by Mike Desaro on 6/5/12.
//  Copyright (c) 2012 prime31. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <VungleSDK/VungleSDK.h>
#import <VungleSDK/VungleSDKHeaderBidding.h>
#import <VungleSDK/VungleSDKCreativeTracking.h>


@interface VungleManager : NSObject <VungleSDKDelegate, VungleSDKHeaderBidding, VungleSDKCreativeTracking>


+ (VungleManager*)sharedManager;

+ (id)objectFromJson:(NSString*)json;

+ (NSString*)jsonFromObject:(id)object;

@end
