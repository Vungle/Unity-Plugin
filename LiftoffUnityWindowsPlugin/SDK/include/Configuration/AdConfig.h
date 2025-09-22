#pragma once

#include <string>

class AdConfig {
public:
    // Gets or sets the title of the confirmation dialog when skipping an incentivized ad.
    std::string IncentivizedDialogTitleText = "";

    // Gets or sets the body of the confirmation dialog when skipping an incentivized ad.
    std::string IncentivizedDialogBodyText = "";

    // Gets or sets the 'cancel button' text of the confirmation dialog when skipping an incentivized ad.
    std::string IncentivizedDialogCloseButtonText = "";

    // Gets or sets the 'continue button' text of the confirmation dialog when skipping an incentivized ad.
    std::string IncentivizedDialogContinueButtonText = "";

    // (Advanced) Offset the ad from the left side of the client's window by this many pixels.
    int XOffset = 0;

    // (Advanced) Offset the ad from the top of the screen by this many pixels.
    int YOffset = 0;

    // Use this to override which window the ad is displayed in.
	// If this is not set, the SDK will use the main window provided during initialization.
    HWND hPublisherWnd;

};
