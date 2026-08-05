//
//  VunglePluginUtil.m
//  UnityFramework
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "VunglePluginUtil.h"

@implementation VunglePluginUtil

+ (const char *)getErrorMessageFromObject:(NSError *)error {
    NSString *message = [NSString stringWithFormat:@"Code: %ld, Description: %@", (long)error.code, error.localizedDescription];
    return [message UTF8String];
}

@end

float VungleGetNativeScreenScale() {
    return (float)[UIScreen mainScreen].scale;
}
