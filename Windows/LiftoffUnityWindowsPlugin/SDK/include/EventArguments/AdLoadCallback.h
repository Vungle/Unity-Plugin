#pragma once

#include "AdLoadEventArgs.h"
#include <functional>

class AdLoadCallback {
public:
	std::function<void(AdLoadEventArgs)> OnAdLoadSuccess;
	std::function<void(AdLoadEventArgs)> OnAdLoadFailure;
	AdLoadCallback() = default;
	// Constructor to initialize the callbacks
	AdLoadCallback(
		std::function<void(AdLoadEventArgs)> onSuccess,
		std::function<void(AdLoadEventArgs)> onFailure)
		: OnAdLoadSuccess(onSuccess), OnAdLoadFailure(onFailure) {
	}
};

