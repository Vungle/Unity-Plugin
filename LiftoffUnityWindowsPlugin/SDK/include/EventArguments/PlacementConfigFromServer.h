#pragma once

#include <string>

class PlacementConfigFromServer {
public:
    // "Friendly" name for a placement; Also called "reference id" (not to be confused with "id"
    std::string Name;
    // Do we automatically download Ad bundles, or must the user call Load()
    bool IsAutoCached;
    // Is this a rewarded Ad?
    bool IsIncentivized;
    // Is this a "mediated" ad, participating in Bidding?
    bool IsHeaderBidding;
    // Max number of bid tokens to cache for this Ad
    int MaxHeaderBiddingCache;

    PlacementConfigFromServer();
};
