#pragma once

#include <string>
#include <exception>

class InitializationFailureEventArgs {
public:
    std::string ErrorMessage;
    std::exception_ptr Exception;

    InitializationFailureEventArgs(const std::string& error)
        : ErrorMessage(error), Exception(nullptr) {}

    InitializationFailureEventArgs(const std::exception& ex)
        : ErrorMessage(ex.what()), Exception(std::make_exception_ptr(ex)) {}
};
