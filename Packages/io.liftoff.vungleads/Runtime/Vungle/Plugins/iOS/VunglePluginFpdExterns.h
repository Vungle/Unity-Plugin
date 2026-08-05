//
//  VunglePluginFpdExterns.h
//  Unity-iPhone
//

void SetVungleFpdAge(int age);
void SetVungleFpdLengthOfResidenceYears(double years);
void SetVungleFpdMedianHomeValueUsd(int amount);
void SetVungleFpdMonthlyHousingPaymentUsd(int amount);
void SetVungleFpdCountry(const char * _Nonnull country);
void SetVungleFpdDma(int dma);
void SetVungleFpdRegionState(const char * _Nonnull regionState);
void SetVungleFpdEarningsByPlacementUsd(double amount);
void SetVungleFpdIsUserAPurchaser(BOOL isPurchaser);
void SetVungleFpdIsUserASubscriber(BOOL isSubscriber);
void SetVungleFpdLast30DaysMeanSpendUsd(double amount);
void SetVungleFpdLast30DaysMedianSpendUsd(double amount);
void SetVungleFpdLast30DaysPlacementFillRate(double fillRate);
void SetVungleFpdLast30DaysTotalSpendUsd(double amount);
void SetVungleFpdLast30DaysUserLtvUsd(double amount);
void SetVungleFpdLast30DaysUserPltvUsd(double amount);
void SetVungleFpdLast7DaysMeanSpendUsd(double amount);
void SetVungleFpdLast7DaysMedianSpendUsd(double amount);
void SetVungleFpdLast7DaysPlacementFillRate(double fillRate);
void SetVungleFpdLast7DaysTotalSpendUsd(double amount);
void SetVungleFpdLast7DaysUserLtvUsd(double amount);
void SetVungleFpdLast7DaysUserPltvUsd(double amount);
void SetVungleFpdTopNAdomain(const char *_Nonnull * _Nonnull strings, int size);
void SetVungleFpdTotalEarningsUsd(double amount);
void SetVungleFpdFriends(const char *_Nonnull * _Nonnull strings, int size);
void SetVungleFpdHealthPercentile(int percentile);
void SetVungleFpdInGamePurchases(double amount);
void SetVungleFpdLevelPercentile(double amount);
void SetVungleFpdPage(const char * _Nonnull page);
void SetVungleFpdSessionStartTime(double epochTime);
void SetVungleFpdSessionDuration(int duration);
void SetVungleFpdSignupDate(double epochTime);
void SetVungleFpdTimeSpent(int duration);
void SetVungleFpdUserId(const char * _Nonnull userId);
void SetVungleFpdUserLevelPercentile(int percentile);
void SetVungleFpdUserScorePercentile(double percentile);
void SetVungleFpdAddCustomData(const char * _Nonnull key, const char * _Nonnull value);
void SetVungleFpdSetCustomData(const char *_Nonnull * _Nonnull keys, const char *_Nonnull * _Nonnull values, int size);
