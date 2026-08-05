package com.vungle.androidplugin;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.vungle.ads.VungleAds;

public class VungleFpdPlugin {
    private static VungleFpdPlugin instance;
    public static VungleFpdPlugin instance() {
        if (instance == null) {
            instance = new VungleFpdPlugin();
        }
        return instance;
    }

    // Demographic
    public void setAgeRange(int age)  {
        VungleAds.firstPartyData.getDemographic().setAgeRange(age);
    }
    public void setLengthOfResidence(int lengthOfResidence) {
        VungleAds.firstPartyData.getDemographic().setLengthOfResidence(lengthOfResidence);
    }
    public void setMedianHomeValueUSD(int homeValue) {
        VungleAds.firstPartyData.getDemographic().setMedianHomeValueUSD(homeValue);
    }
    public void setMonthlyHousingCosts(int housingCost) {
        VungleAds.firstPartyData.getDemographic().setMonthlyHousingCosts(housingCost);
    }

    // Location
    public void setCountry(String country)  {
        VungleAds.firstPartyData.getLocation().setCountry(country);
    }
    public void setRegionState(String regionState)  {
        VungleAds.firstPartyData.getLocation().setRegionState(regionState);
    }
    public void setDma(int dma)  {
        VungleAds.firstPartyData.getLocation().setDma(dma);
    }

    // Revenue
    public void setTotalEarningsUsd(float totalEarningsUsd)  {
        VungleAds.firstPartyData.getRevenue().setTotalEarningsUsd(totalEarningsUsd);
    }
    public void setEarningsByPlacement(float earningsByPlacement)  {
        VungleAds.firstPartyData.getRevenue().setEarningsByPlacement(earningsByPlacement);
    }
    public void setTopNAdomain(String[] topNAdomain)  {
        List<String> topNAdomainList = Arrays.asList(topNAdomain);
        VungleAds.firstPartyData.getRevenue().setTopNAdomain(topNAdomainList);
    }
    public void setIsUserAPurchaser(boolean isUserAPurchaser)  {
        VungleAds.firstPartyData.getRevenue().setIsUserAPurchaser(isUserAPurchaser);
    }
    public void setIsUserASubscriber(boolean isUserASubscriber)  {
        VungleAds.firstPartyData.getRevenue().setIsUserASubscriber(isUserASubscriber);
    }
    public void setLast7DaysMedianSpendUsd(float last7DaysMedianSpendUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast7DaysMedianSpendUsd(last7DaysMedianSpendUsd);
    }
    public void setLast7DaysTotalSpendUsd(float last7DaysTotalSpendUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast7DaysTotalSpendUsd(last7DaysTotalSpendUsd);
    }
    public void setLast30DaysTotalSpendUsd(float last30DaysTotalSpendUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast30DaysTotalSpendUsd(last30DaysTotalSpendUsd);
    }
    public void setLast7DaysMeanSpendUsd(float last7DaysMeanSpendUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast7DaysMeanSpendUsd(last7DaysMeanSpendUsd);
    }
    public void setLast30DaysMedianSpendUsd(float last30DaysMedianSpendUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast30DaysMedianSpendUsd(last30DaysMedianSpendUsd);
    }
    public void setLast30DaysMeanSpendUsd(float last30DaysMeanSpendUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast30DaysMeanSpendUsd(last30DaysMeanSpendUsd);
    }
    public void setLast7DaysUserPltvUsd(float last7DaysUserPltvUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast7DaysUserPltvUsd(last7DaysUserPltvUsd);
    }
    public void setLast7DaysUserLtvUsd(float last7DaysUserLtvUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast7DaysUserLtvUsd(last7DaysUserLtvUsd);
    }
    public void setLast30DaysUserPltvUsd(float last30DaysUserPltvUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast30DaysUserPltvUsd(last30DaysUserPltvUsd);
    }
    public void setLast30DaysUserLtvUsd(float last30DaysUserLtvUsd)  {
        VungleAds.firstPartyData.getRevenue().setLast30DaysUserLtvUsd(last30DaysUserLtvUsd);
    }
    public void setLast7DaysPlacementFillRate(float last7DaysPlacementFillRate)  {
        VungleAds.firstPartyData.getRevenue().setLast7DaysPlacementFillRate(last7DaysPlacementFillRate);
    }
    public void setLast30DaysPlacementFillRate(float last30DaysPlacementFillRate)  {
        VungleAds.firstPartyData.getRevenue().setLast30DaysPlacementFillRate(last30DaysPlacementFillRate);
    }

    // SessionContext
    public void setLevelPercentile(float levelPercentile)  {
        VungleAds.firstPartyData.getSessionContext().setLevelPercentile(levelPercentile);
    }
    public void setPage(String page)  {
        VungleAds.firstPartyData.getSessionContext().setPage(page);
    }
    public void setTimeSpent(int timeSpent)  {
        VungleAds.firstPartyData.getSessionContext().setTimeSpent(timeSpent);
    }
    public void setSignupDate(int signupDate)  {
        VungleAds.firstPartyData.getSessionContext().setSignupDate(signupDate);
    }
    public void setUserScorePercentile(float userScorePercentile)  {
        VungleAds.firstPartyData.getSessionContext().setUserScorePercentile(userScorePercentile);
    }
    public void setUserID(String userID)  {
        VungleAds.firstPartyData.getSessionContext().setUserID(userID);
    }
    public void setFriends(String[] friends)  {
        List<String> friendsList = Arrays.asList(friends);
        VungleAds.firstPartyData.getSessionContext().setFriends(friendsList);
    }
    public void setUserLevelPercentile(float userLevelPercentile)  {
        VungleAds.firstPartyData.getSessionContext().setUserLevelPercentile(userLevelPercentile);
    }
    public void setHealthPercentile(float healthPercentile)  {
        VungleAds.firstPartyData.getSessionContext().setHealthPercentile(healthPercentile);
    }
    public void setSessionStartTime(int sessionStartTime)  {
        VungleAds.firstPartyData.getSessionContext().setSessionStartTime(sessionStartTime);
    }
    public void setSessionDuration(int sessionDuration)  {
        VungleAds.firstPartyData.getSessionContext().setSessionDuration(sessionDuration);
    }
    public void setInGamePurchasesUSD(float inGamePurchasesUSD)  {
        VungleAds.firstPartyData.getSessionContext().setInGamePurchasesUSD(inGamePurchasesUSD);
    }

    // CustomData
    public void addCustomData(String key, String value)  {
        VungleAds.firstPartyData.getCustomData().put(key, value);
    }
    public void setCustomData(String[] keys, String[] values, int count)  {
        Map<String, String> customData = new HashMap<>();
        for (int i = 0; i < count; i++) {
            customData.put(keys[i], values[i]);
        }
        VungleAds.firstPartyData.getCustomData().putAll(customData);
    }
}
