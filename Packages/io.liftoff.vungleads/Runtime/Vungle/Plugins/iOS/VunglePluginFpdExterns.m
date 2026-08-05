//
//  VunglePluginFpdExterns.m
//  UnityFramework
//

#import <Foundation/Foundation.h>
#import "VunglePluginFpdExterns.h"
#import "VunglePluginFpd.h"

#define GetStringParam(_x_) (_x_ != NULL) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]
#define GetDecimalParam(_x_) [[[NSDecimalNumber alloc] initWithDouble:_x_] decimalValue]

NSMutableArray<NSString *>* ConvertCStringToNSStringsArray(const char **strings, int size) {
    NSMutableArray<NSString *> *array = [NSMutableArray arrayWithCapacity:size];
    for (int i = 0; i < size; i++) {
        NSString *nsString = [NSString stringWithUTF8String:strings[i]];
        [array addObject:nsString];
    }
    return array;
}

NSDictionary<NSString *, NSString *>* ConvertCStringToNSStringsDictionary(const char **keys, const char **values, int size) {
    NSMutableDictionary<NSString *, NSString *> *dict = [NSMutableDictionary dictionaryWithCapacity:size];
    for (int i = 0; i < size; i++) {
        NSString *key = [NSString stringWithUTF8String:keys[i]];
        NSString *value = [NSString stringWithUTF8String:values[i]];
        dict[key] = value;
    }
    return dict;
}

void SetVungleFpdAge(int age) {
    [VunglePluginFpd setAge:age];
}

void SetVungleFpdLengthOfResidenceYears(double years) {
    [VunglePluginFpd setLengthOfResidenceYears:GetDecimalParam(years)];
}

void SetVungleFpdMedianHomeValueUsd(int amount) {
    [VunglePluginFpd setMedianHomeValueUsd:amount];
}

void SetVungleFpdMonthlyHousingPaymentUsd(int amount) {
    [VunglePluginFpd setMonthlyHousingPaymentUsd:amount];
}

void SetVungleFpdCountry(const char * _Nonnull country) {
    [VunglePluginFpd setCountry:GetStringParam(country)];
}

void SetVungleFpdDma(int dma) {
    [VunglePluginFpd setDma:dma];
}

void SetVungleFpdRegionState(const char * _Nonnull regionState) {
    [VunglePluginFpd setRegionState:GetStringParam(regionState)];
}

void SetVungleFpdEarningsByPlacementUsd(double amount) {
    [VunglePluginFpd setEarningsByPlacementUsd:GetDecimalParam(amount)];
}

void SetVungleFpdIsUserAPurchaser(BOOL isPurchaser) {
    [VunglePluginFpd setIsUserAPurchaser:isPurchaser];
}

void SetVungleFpdIsUserASubscriber(BOOL isSubscriber) {
    [VunglePluginFpd setIsUserASubscriber:isSubscriber];
}

void SetVungleFpdLast30DaysMeanSpendUsd(double amount) {
    [VunglePluginFpd setLast30DaysMeanSpendUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast30DaysMedianSpendUsd(double amount) {
    [VunglePluginFpd setLast30DaysMedianSpendUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast30DaysPlacementFillRate(double fillRate) {
    [VunglePluginFpd setLast30DaysPlacementFillRate:GetDecimalParam(fillRate)];
}

void SetVungleFpdLast30DaysTotalSpendUsd(double amount) {
    [VunglePluginFpd setLast30DaysTotalSpendUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast30DaysUserLtvUsd(double amount) {
    [VunglePluginFpd setLast30DaysUserLtvUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast30DaysUserPltvUsd(double amount) {
    [VunglePluginFpd setLast30DaysUserPltvUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast7DaysMeanSpendUsd(double amount) {
    [VunglePluginFpd setLast7DaysMeanSpendUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast7DaysMedianSpendUsd(double amount) {
    [VunglePluginFpd setLast7DaysMedianSpendUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast7DaysPlacementFillRate(double fillRate) {
    [VunglePluginFpd setLast7DaysPlacementFillRate:GetDecimalParam(fillRate)];
}

void SetVungleFpdLast7DaysTotalSpendUsd(double amount) {
    [VunglePluginFpd setLast7DaysTotalSpendUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast7DaysUserLtvUsd(double amount) {
    [VunglePluginFpd setLast7DaysUserLtvUsd:GetDecimalParam(amount)];
}

void SetVungleFpdLast7DaysUserPltvUsd(double amount) {
    [VunglePluginFpd setLast7DaysUserPltvUsd:GetDecimalParam(amount)];
}

void SetVungleFpdTopNAdomain(const char *_Nonnull * _Nonnull strings, int size) {
    [VunglePluginFpd setTopNAdomain:ConvertCStringToNSStringsArray(strings, size)];
}

void SetVungleFpdTotalEarningsUsd(double amount) {
    [VunglePluginFpd setTotalEarningsUsd:GetDecimalParam(amount)];
}

void SetVungleFpdFriends(const char *_Nonnull * _Nonnull strings, int size) {
    [VunglePluginFpd setFriends:ConvertCStringToNSStringsArray(strings, size)];
}

void SetVungleFpdHealthPercentile(int percentile) {
    [VunglePluginFpd setHealthPercentile:percentile];
}

void SetVungleFpdInGamePurchases(double amount) {
    [VunglePluginFpd setInGamePurchases:GetDecimalParam(amount)];
}

void SetVungleFpdLevelPercentile(double amount) {
    [VunglePluginFpd setLevelPercentile:GetDecimalParam(amount)];
}

void SetVungleFpdPage(const char * _Nonnull page) {
    [VunglePluginFpd setPage:GetStringParam(page)];
}

void SetVungleFpdSessionStartTime(double epochTime) {
    [VunglePluginFpd setSessionStartTime:[NSDate dateWithTimeIntervalSince1970:epochTime]];
}

void SetVungleFpdSessionDuration(int duration) {
    [VunglePluginFpd setSessionDuration:duration];
}

void SetVungleFpdSignupDate(double epochTime) {
    [VunglePluginFpd setSignupDate:[NSDate dateWithTimeIntervalSince1970:epochTime]];
}

void SetVungleFpdTimeSpent(int duration) {
    [VunglePluginFpd setTimeSpent:duration];
}

void SetVungleFpdUserId(const char * _Nonnull userId) {
    [VunglePluginFpd setUserId:GetStringParam(userId)];
}

void SetVungleFpdUserLevelPercentile(int percentile) {
    [VunglePluginFpd setUserLevelPercentile:percentile];
}

void SetVungleFpdUserScorePercentile(double percentile) {
    [VunglePluginFpd setUserScorePercentile:GetDecimalParam(percentile)];
}

void SetVungleFpdAddCustomData(const char * _Nonnull key, const char * _Nonnull value) {
    [VunglePluginFpd addCustomData:GetStringParam(key) value:GetStringParam(value)];
}

void SetVungleFpdSetCustomData(const char *_Nonnull * _Nonnull keys, const char *_Nonnull * _Nonnull values, int size) {
    [VunglePluginFpd setCustomData:ConvertCStringToNSStringsDictionary(keys, values, size)];
}
