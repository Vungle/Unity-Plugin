#pragma once

#include <string>
#include <sstream>

class AdLoadEventArgs {
public:
    // Indicates availability of the ad
    bool AdPlayable;
    std::string Placement;
    std::string ErrorMessage; // Optional error message for debugging

    AdLoadEventArgs(bool adPlayable, const std::string& placement)
        : AdPlayable(adPlayable), Placement(placement) {}

    AdLoadEventArgs(bool adPlayable, const std::string& placement, const std::string& errorMessage)
        : AdPlayable(adPlayable), Placement(placement), ErrorMessage(errorMessage) {
    }

    std::string ToString() const {
        std::ostringstream oss;
        oss << "AdLoadEventArgs Placement: " << Placement << "; Playable: " << AdPlayable;
        return oss.str();
    }
};
