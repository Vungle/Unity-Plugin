#pragma once

#include <string>
#include <sstream>

class AdLoadEventArgs {
public:
    // Indicates availability of the ad
    bool AdPlayable;
    std::string Placement;
    std::string ErrorMessage; // Optional error message for debugging
	int BundleCount = 0; // Optional, number of ad bundles currently available

    AdLoadEventArgs(bool adPlayable, const std::string& placement, int bundleCount)
        : AdPlayable(adPlayable), Placement(placement), BundleCount(bundleCount) {}

    AdLoadEventArgs(bool adPlayable, const std::string& placement, const std::string& errorMessage, int bundleCount)
        : AdPlayable(adPlayable), Placement(placement), ErrorMessage(errorMessage), BundleCount(bundleCount) {
    }

    std::string ToString() const {
        std::ostringstream oss;
        oss << "AdLoadEventArgs Placement: " << Placement << "; Playable: " << AdPlayable << "; BundleCount: " << BundleCount;
        return oss.str();
    }
};
