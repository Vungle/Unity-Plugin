#pragma once

#include "InitializationSuccessEventArgs.h"
#include "InitializationFailureEventArgs.h"
#include <functional>

// Create a class that has two properties. Both should be std::function<void(...)>.
// One should be called OnInitializationSuccess, and the other should be called OnInitializationFailure.
class InitializationCallback {
public:
    std::function<void(InitializationSuccessEventArgs)> OnInitializationSuccess;
    std::function<void(InitializationFailureEventArgs)> OnInitializationFailure;

    InitializationCallback() = default;

    // Constructor to initialize the callbacks
    InitializationCallback(
        std::function<void(InitializationSuccessEventArgs)> onSuccess,
        std::function<void(InitializationFailureEventArgs)> onFailure)
        : OnInitializationSuccess(onSuccess), OnInitializationFailure(onFailure) {
    }
};
