#pragma once

#include <string>
#include <sstream>

#include "../EventArguments/AdPlayCallback.h"

class LiftoffAdPlayInfo {
public:
    // Were we able to successfully begin playing the ad
    bool Success = true;
    // Which placement were we trying to play an ad for
    std::string Placement = "";
    // What was the error message, if any, if something went wrong with ad play
    std::string ErrorMessage = "";
    // What was the Vungle event ID for this ad play; this can be used to report back to Vungle
    std::string VungleEventId = "";

    std::string ToString() const {
        std::ostringstream oss;
        oss << "PlayAd returned: Success " << (Success ? "true" : "false")
            << "; Placement " << Placement
            << ", Vungle Event Id: " << VungleEventId
            << ", Error Message " << ErrorMessage;
        return oss.str();
    }

	// TODO : Does this break "separate of concerns"?  Would it be useful to the publisher
	// to have this returned from PlayAdAsync?
    AdPlayCallback Callbacks;
};
