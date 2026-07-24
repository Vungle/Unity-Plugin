using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace VungleAds
{
    public static class VungleFpd {
    #if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject fpdObject;

        static VungleFpd()
        {
            using var vungleClass = new AndroidJavaClass("com.vungle.androidplugin.VungleFpdPlugin");
            fpdObject = vungleClass.CallStatic<AndroidJavaObject>("instance");
        }
    #endif

        public static void SetAge(int age)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setAgeRange", age);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdAge(age);
    #endif
        }

        public static void SetLengthOfResidenceYears(double years)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLengthOfResidence", (int)years);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLengthOfResidenceYears(years);
    #endif
        }

        public static void SetMedianHomeValueUsd(int amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setMedianHomeValueUSD", amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdMedianHomeValueUsd(amount);
    #endif
        }

        public static void SetMonthlyHousingPaymentUsd(int amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setMonthlyHousingCosts", amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdMonthlyHousingPaymentUsd(amount);
    #endif
        }

        public static void SetCountry(string country)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setCountry", country);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdCountry(country);
    #endif
        }

        public static void SetDma(int dma)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setDma", dma);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdDma(dma);
    #endif
        }

        public static void SetRegionState(string regionState)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setRegionState", regionState);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdRegionState(regionState);
    #endif
        }

        public static void SetEarningsByPlacementUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setEarningsByPlacement", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdEarningsByPlacementUsd(amount);
    #endif
        }

        public static void SetIsUserAPurchaser(bool isPurchaser)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setIsUserAPurchaser", isPurchaser);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdIsUserAPurchaser(isPurchaser);
    #endif
        }

        public static void SetIsUserASubscriber(bool isSubscriber)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setIsUserASubscriber", isSubscriber);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdIsUserASubscriber(isSubscriber);
    #endif
        }

        public static void SetLast30DaysMeanSpendUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast30DaysMeanSpendUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast30DaysMeanSpendUsd(amount);
    #endif
        }

        public static void SetLast30DaysMedianSpendUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast30DaysMedianSpendUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast30DaysMedianSpendUsd(amount);
    #endif
        }

        public static void SetLast30DaysPlacementFillRate(double fillRate)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast30DaysPlacementFillRate", (float)fillRate);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast30DaysPlacementFillRate(fillRate);
    #endif
        }

        public static void SetLast30DaysTotalSpendUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast30DaysTotalSpendUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast30DaysTotalSpendUsd(amount);
    #endif
        }

        public static void SetLast30DaysUserLtvUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast30DaysUserLtvUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast30DaysUserLtvUsd(amount);
    #endif
        }

        public static void SetLast30DaysUserPltvUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast30DaysUserPltvUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast30DaysUserPltvUsd(amount);
    #endif
        }

        public static void SetLast7DaysMeanSpendUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast7DaysMeanSpendUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast7DaysMeanSpendUsd(amount);
    #endif
        }

        public static void SetLast7DaysMedianSpendUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast7DaysMedianSpendUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast7DaysMedianSpendUsd(amount);
    #endif
        }

        public static void SetLast7DaysPlacementFillRate(double fillRate)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast7DaysPlacementFillRate", (float)fillRate);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast7DaysPlacementFillRate(fillRate);
    #endif
        }

        public static void SetLast7DaysTotalSpendUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast7DaysTotalSpendUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast7DaysTotalSpendUsd(amount);
    #endif
        }

        public static void SetLast7DaysUserLtvUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast7DaysUserLtvUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast7DaysUserLtvUsd(amount);
    #endif
        }

        public static void SetLast7DaysUserPltvUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLast7DaysUserPltvUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLast7DaysUserPltvUsd(amount);
    #endif
        }

        public static void SetTopNAdomain(List<string> domains)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            string[] strings = domains.ToArray();
            fpdObject.Call("setTopNAdomain", strings);
    #elif UNITY_IOS && !UNITY_EDITOR
            int size = domains.Count;
            string[] strings = domains.ToArray();
            SetVungleFpdTopNAdomain(strings, size);
    #endif
        }

        public static void SetTotalEarningsUsd(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setTotalEarningsUsd", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdTotalEarningsUsd(amount);
    #endif
        }

        public static void SetFriends(List<string> friends)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            string[] strings = friends.ToArray();
            fpdObject.Call("setFriends", strings);
    #elif UNITY_IOS && !UNITY_EDITOR
            int size = friends.Count;
            string[] strings = friends.ToArray();
            SetVungleFpdFriends(strings, size);
    #endif
        }

        public static void SetHealthPercentile(int percentile)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setHealthPercentile", (float)percentile);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdHealthPercentile(percentile);
    #endif
        }

        public static void SetInGamePurchases(double amount)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setInGamePurchasesUSD", (float)amount);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdInGamePurchases(amount);
    #endif
        }

        public static void SetLevelPercentile(double percentile)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setLevelPercentile", (float)percentile);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdLevelPercentile(percentile);
    #endif
        }

        public static void SetPage(string page)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setPage", page);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdPage(page);
    #endif
        }

        public static void SetSessionStartTime(DateTime startTime)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            double timestamp = (startTime - new DateTime(1970, 1, 1)).TotalSeconds;
            fpdObject.Call("setSessionStartTime", (int)timestamp);
    #elif UNITY_IOS && !UNITY_EDITOR
            double timestamp = (startTime - new DateTime(1970, 1, 1)).TotalSeconds;
            SetVungleFpdSessionStartTime(timestamp);
    #endif
        }

        public static void SetSessionDuration(int duration)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setSessionDuration", duration);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdSessionDuration(duration);
    #endif
        }

        public static void SetSignupDate(DateTime signupDate)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            double timestamp = (signupDate - new DateTime(1970, 1, 1)).TotalSeconds;
            fpdObject.Call("setSignupDate", (int)timestamp);
    #elif UNITY_IOS && !UNITY_EDITOR
            double timestamp = (signupDate - new DateTime(1970, 1, 1)).TotalSeconds;
            SetVungleFpdSignupDate(timestamp);
    #endif
        }

        public static void SetTimeSpent(int duration)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setTimeSpent", duration);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdTimeSpent(duration);
    #endif
        }

        public static void SetUserId(string userId)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setUserID", userId);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdUserId(userId);
    #endif
        }

        public static void SetUserLevelPercentile(int percentile)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setUserLevelPercentile", (float)percentile);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdUserLevelPercentile(percentile);
    #endif
        }

        public static void SetUserScorePercentile(double percentile)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("setUserScorePercentile", (float)percentile);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdUserScorePercentile(percentile);
    #endif
        }

        public static void AddCustomData(string key, string value)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            fpdObject.Call("addCustomData", key, value);
    #elif UNITY_IOS && !UNITY_EDITOR
            SetVungleFpdAddCustomData(key, value);
    #endif
        }

        public static void SetCustomData(Dictionary<string, string> dictionary)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            int size = dictionary.Count;
            string[] keys = new string[size];
            string[] values = new string[size];

            int index = 0;
            foreach (var kvp in dictionary)
            {
                keys[index] = kvp.Key;
                values[index] = kvp.Value;
                index++;
            }

            fpdObject.Call("setCustomData", keys, values, size);
    #elif UNITY_IOS && !UNITY_EDITOR
            int size = dictionary.Count;
            string[] keys = new string[size];
            string[] values = new string[size];

            int index = 0;
            foreach (var kvp in dictionary)
            {
                keys[index] = kvp.Key;
                values[index] = kvp.Value;
                index++;
            }

            SetVungleFpdSetCustomData(keys, values, size);
    #endif
        }

    #if UNITY_ANDROID && !UNITY_EDITOR

    #elif UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        internal static extern void SetVungleFpdAge(int age);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLengthOfResidenceYears(double years);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdMedianHomeValueUsd(int amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdMonthlyHousingPaymentUsd(int amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdCountry(string country);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdDma(int dma);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdRegionState(string regionState);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdEarningsByPlacementUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdIsUserAPurchaser(bool isPurchaser);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdIsUserASubscriber(bool isSubscriber);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast30DaysMeanSpendUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast30DaysMedianSpendUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast30DaysPlacementFillRate(double fillRate);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast30DaysTotalSpendUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast30DaysUserLtvUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast30DaysUserPltvUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast7DaysMeanSpendUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast7DaysMedianSpendUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast7DaysPlacementFillRate(double fillRate);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast7DaysTotalSpendUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast7DaysUserLtvUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLast7DaysUserPltvUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdTopNAdomain(string[] strings, int size);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdTotalEarningsUsd(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdFriends(string[] strings, int size);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdHealthPercentile(int percentile);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdInGamePurchases(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdLevelPercentile(double amount);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdPage(string page);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdSessionStartTime(double epochTime);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdSessionDuration(int duration);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdSignupDate(double epochTime);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdTimeSpent(int duration);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdUserId(string userId);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdUserLevelPercentile(int percentile);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdUserScorePercentile(double percentile);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdAddCustomData(string key, string value);

        [DllImport("__Internal")]
        internal static extern void SetVungleFpdSetCustomData(string[] keys, string[] values, int size);
    #endif
    }
}
