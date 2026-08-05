//
//  VunglePluginFpd.m
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "VunglePluginFpd.h"

@interface VunglePluginFpd ()

@end

@implementation VunglePluginFpd

+ (void)setAge:(int)age {
    [[VungleAds firstPartyData] setAge:age];
}

+ (void)setLengthOfResidenceYears:(NSDecimal)years {
    [[VungleAds firstPartyData] setLengthOfResidenceYears:years];
}

+ (void)setMedianHomeValueUsd:(int)amount {
    [[VungleAds firstPartyData] setMedianHomeValueUsd:amount];
}

+ (void)setMonthlyHousingPaymentUsd:(int)amount {
    [[VungleAds firstPartyData] setMonthlyHousingPaymentUsd:amount];
}

+ (void)setCountry:(NSString *)country {
    [[VungleAds firstPartyData] setCountry:country];
}

+ (void)setDma:(int)dma {
    [[VungleAds firstPartyData] setDma:dma];
}

+ (void)setRegionState:(NSString *)regionState {
    [[VungleAds firstPartyData] setRegionState:regionState];
}

+ (void)setEarningsByPlacementUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setEarningsByPlacement:amount];
}

+ (void)setIsUserAPurchaser:(BOOL)isPurchaser {
    [[VungleAds firstPartyData] setIsUserAPurchaser:isPurchaser];
}

+ (void)setIsUserASubscriber:(BOOL)isSubscriber {
    [[VungleAds firstPartyData] setIsUserASubscriber:isSubscriber];
}

+ (void)setLast30DaysMeanSpendUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast30DaysMeanSpendUsd:amount];
}

+ (void)setLast30DaysMedianSpendUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast30DaysMedianSpendUsd:amount];
}

+ (void)setLast30DaysPlacementFillRate:(NSDecimal)fillRate {
    [[VungleAds firstPartyData] setLast30DaysPlacementFillRate:fillRate];
}

+ (void)setLast30DaysTotalSpendUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast30DaysTotalSpendUsd:amount];
}

+ (void)setLast30DaysUserLtvUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast30DaysUserLtvUsd:amount];
}

+ (void)setLast30DaysUserPltvUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast30DaysUserPltvUsd:amount];
}

+ (void)setLast7DaysMeanSpendUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast7DaysMeanSpendUsd:amount];
}

+ (void)setLast7DaysMedianSpendUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast7DaysMedianSpendUsd:amount];
}

+ (void)setLast7DaysPlacementFillRate:(NSDecimal)fillRate {
    [[VungleAds firstPartyData] setLast7DaysPlacementFillRate:fillRate];
}

+ (void)setLast7DaysTotalSpendUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast7DaysTotalSpendUsd:amount];
}

+ (void)setLast7DaysUserLtvUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast7DaysUserLtvUsd:amount];
}

+ (void)setLast7DaysUserPltvUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLast7DaysUserPltvUsd:amount];
}

+ (void)setTopNAdomain:(NSMutableArray<NSString *> *)strings {
    [[VungleAds firstPartyData] setTopNAdomain:strings];
}

+ (void)setTotalEarningsUsd:(NSDecimal)amount {
    [[VungleAds firstPartyData] setTotalEarningsUsd:amount];
}

+ (void)setFriends:(NSArray<NSString *> *)strings {
    [[VungleAds firstPartyData] setFriends:strings];
}

+ (void)setHealthPercentile:(int)percentile {
    [[VungleAds firstPartyData] setHealthPercentile:percentile];
}

+ (void)setInGamePurchases:(NSDecimal)amount {
    [[VungleAds firstPartyData] setInGamePurchases:amount];
}

+ (void)setLevelPercentile:(NSDecimal)amount {
    [[VungleAds firstPartyData] setLevelPercentile:amount];
}

+ (void)setPage:(NSString *)page {
    [[VungleAds firstPartyData] setPage:page];
}

+ (void)setSessionStartTime:(NSDate *)date {
    [[VungleAds firstPartyData] setSessionStartTime:date];
}

+ (void)setSessionDuration:(int)duration {
    [[VungleAds firstPartyData] setSessionDuration:duration];
}

+ (void)setSignupDate:(NSDate *)date {
    [[VungleAds firstPartyData] setSignupDate:date];
}

+ (void)setTimeSpent:(int)duration {
    [[VungleAds firstPartyData] setTimeSpent:duration];
}

+ (void)setUserId:(NSString *)userId {
    [[VungleAds firstPartyData] setUserId:userId];
}

+ (void)setUserLevelPercentile:(int)percentile {
    [[VungleAds firstPartyData] setUserLevelPercentile:percentile];
}

+ (void)setUserScorePercentile:(NSDecimal)percentile {
    [[VungleAds firstPartyData] setUserScorePercentile:percentile];
}

+ (void)addCustomData:(NSString *)key value:(NSString *)value {
    [[VungleAds firstPartyData] addCustomData:key value:value];
}

+ (void)setCustomData:(NSDictionary<NSString *, NSString *> *)dict {
    [[VungleAds firstPartyData] setCustomData:dict];
}

@end
