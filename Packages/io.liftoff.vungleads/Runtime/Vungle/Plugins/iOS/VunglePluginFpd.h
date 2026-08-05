//
//  VunglePluginFpd.h
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <VungleAdsSDK/VungleAdsSDK-Swift.h>
#import "VunglePluginFpdExterns.h"

@interface VunglePluginFpd : NSObject

+ (void)setAge:(int)age;
+ (void)setLengthOfResidenceYears:(NSDecimal)years;
+ (void)setMedianHomeValueUsd:(int)amount;
+ (void)setMonthlyHousingPaymentUsd:(int)amount;
+ (void)setCountry:(NSString *)country;
+ (void)setDma:(int)dma;
+ (void)setRegionState:(NSString *)regionState;
+ (void)setEarningsByPlacementUsd:(NSDecimal)amount;
+ (void)setIsUserAPurchaser:(BOOL)isPurchaser;
+ (void)setIsUserASubscriber:(BOOL)isSubscriber;
+ (void)setLast30DaysMeanSpendUsd:(NSDecimal)amount;
+ (void)setLast30DaysMedianSpendUsd:(NSDecimal)amount;
+ (void)setLast30DaysPlacementFillRate:(NSDecimal)fillRate;
+ (void)setLast30DaysTotalSpendUsd:(NSDecimal)amount;
+ (void)setLast30DaysUserLtvUsd:(NSDecimal)amount;
+ (void)setLast30DaysUserPltvUsd:(NSDecimal)amount;
+ (void)setLast7DaysMeanSpendUsd:(NSDecimal)amount;
+ (void)setLast7DaysMedianSpendUsd:(NSDecimal)amount;
+ (void)setLast7DaysPlacementFillRate:(NSDecimal)fillRate;
+ (void)setLast7DaysTotalSpendUsd:(NSDecimal)amount;
+ (void)setLast7DaysUserLtvUsd:(NSDecimal)amount;
+ (void)setLast7DaysUserPltvUsd:(NSDecimal)amount;
+ (void)setTopNAdomain:(NSMutableArray<NSString *> *)strings;
+ (void)setTotalEarningsUsd:(NSDecimal)amount;
+ (void)setFriends:(NSArray<NSString *> *)strings;
+ (void)setHealthPercentile:(int)percentile;
+ (void)setInGamePurchases:(NSDecimal)amount;
+ (void)setLevelPercentile:(NSDecimal)amount;
+ (void)setPage:(NSString *)page;
+ (void)setSessionStartTime:(NSDate *)date;
+ (void)setSessionDuration:(int)duration;
+ (void)setSignupDate:(NSDate *)date;
+ (void)setTimeSpent:(int)duration;
+ (void)setUserId:(NSString *)userId;
+ (void)setUserLevelPercentile:(int)percentile;
+ (void)setUserScorePercentile:(NSDecimal)percentile;
+ (void)addCustomData:(NSString *)key value:(NSString *)value;
+ (void)setCustomData:(NSDictionary<NSString *, NSString *> *)dict;

@end
