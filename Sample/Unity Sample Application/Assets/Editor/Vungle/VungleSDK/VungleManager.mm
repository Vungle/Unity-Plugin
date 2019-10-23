//
//  VungleManager.m
//  VungleTest
//
//  Created by Mike Desaro on 6/5/12.
//  Copyright (c) 2012 prime31. All rights reserved.
//

#import "VungleManager.h"
#import <objc/runtime.h>


#if UNITY_VERSION < 500
void UnityPause( bool pause );
#else
void UnityPause( int pause );
#endif

UIViewController *UnityGetGLViewController();

void UnitySendMessage( const char * className, const char * methodName, const char * param );



#if __has_feature(objc_arc)
#define SAFE_ARC_AUTORELEASE(x) (x)
#else
#define SAFE_ARC_AUTORELEASE(x) ([(x) autorelease])
#endif


@implementation VungleManager

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Class Methods

+ (VungleManager*)sharedManager
{
	static VungleManager *sharedSingleton;
	
	if( !sharedSingleton )
		sharedSingleton = [[VungleManager alloc] init];
	
	return sharedSingleton;
}


+ (NSString*)jsonFromObject:(id)object
{
	NSError *error = nil;
	NSData *jsonData = [NSJSONSerialization dataWithJSONObject:object options:0 error:&error];
	
	if( jsonData && !error )
		return SAFE_ARC_AUTORELEASE( [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] );
	else
		NSLog( @"jsonData was null, error: %@", [error localizedDescription] );

    return @"{}";
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - Public

+ (id)objectFromJson:(NSString*)json
{
    NSError *error = nil;
    NSData *data = [NSData dataWithBytes:json.UTF8String length:json.length];
    NSObject *object = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingAllowFragments error:&error];
    
    if( error )
        NSLog( @"failed to deserialize JSON: %@ with error: %@", json, [error localizedDescription] );
    
    return object;
}

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark - VGVunglePubDelegate

- (BOOL)vungleWillShowAdForPlacementID:(nullable NSString *)placementID {
    UnitySendMessage("VungleManager", "OnAdStart", placementID?[placementID UTF8String]:"");
    return true;
}

- (void)vungleAdPlayabilityUpdate:(BOOL)isAdPlayable placementID:(nullable NSString *)placementID error:(nullable NSError *)error {
    NSDictionary *dict = @{
                           @"isAdAvailable": [NSNumber numberWithBool:isAdPlayable],
                           @"placementID": placementID ?: @""
                           };
    UnitySendMessage("VungleManager", "OnAdPlayable", [VungleManager jsonFromObject:dict].UTF8String);
}

- (void)vungleDidCloseAdWithViewInfo:(nonnull VungleViewInfo *)info placementID:(nonnull NSString *)placementID {
    NSDictionary *dict = @{
                           @"completedView": [info completedView] ?: [NSNull null],
                           @"playTime": [info playTime] ?: [NSNull null],
                           @"didDownload": [info didDownload] ?: [NSNull null],
                           @"placementID": placementID ?: @""
                           };
	UnitySendMessage("VungleManager", "OnAdEnd", [VungleManager jsonFromObject:dict].UTF8String);
}

- (void)vungleSDKDidInitialize {
    UnitySendMessage( "VungleManager", "OnInitialize", "" );
}

- (void)vungleSDKLog:(NSString*)message
{
       	UnitySendMessage( "VungleManager", "OnSDKLog", [message UTF8String]);
}

- (void)placementPrepared:(NSString *)placement withBidToken:(NSString *)bidToken {
        NSDictionary *dict = @{
                           @"placementID": placement ?: @"",
                           @"bidToken": bidToken ?: @""
                           };
	UnitySendMessage("VungleManager", "OnPlacementPrepared", [VungleManager jsonFromObject:dict].UTF8String);
}

- (void)vungleCreative:(nullable NSString *)creativeID readyForPlacement:(nullable NSString *)placementID {
        NSDictionary *dict = @{
                           @"placementID": placementID ?: @"",
                           @"creativeID": creativeID ?: @""
                           };
	UnitySendMessage("VungleManager", "OnVungleCreative", [VungleManager jsonFromObject:dict].UTF8String);
}

@end
