#pragma once

#include "AdPlayEventArgs.h"
#include <functional>

class AdPlayCallback {
public:
	std::function<void(const AdPlayEventArgs)> OnAdStart;
	std::function<void(const AdPlayEventArgs)> OnAdEnd;
	std::function<void(const AdPlayEventArgs)> OnAdPlayClick;
	std::function<void(const AdPlayEventArgs)> OnAdPlayRewarded;
	std::function<void(const AdPlayEventArgs)> OnAdPlayFailure;
	AdPlayCallback() = default;
	// Constructor to initialize the callbacks
	AdPlayCallback(
		std::function<void(const AdPlayEventArgs)> onAdStart,
		std::function<void(const AdPlayEventArgs)> onAdEnd,
		std::function<void(const AdPlayEventArgs)> onAdClick,
		std::function<void(const AdPlayEventArgs)> onAdRewarded,
		std::function<void(const AdPlayEventArgs)> onAdPlayFailure)
		: OnAdStart(onAdStart),
		OnAdEnd(onAdEnd),
		OnAdPlayClick(onAdClick),
		OnAdPlayRewarded(onAdRewarded),
		OnAdPlayFailure(onAdPlayFailure) {
	}
};
