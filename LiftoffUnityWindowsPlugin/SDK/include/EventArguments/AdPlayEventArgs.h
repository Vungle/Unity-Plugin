#pragma once

#include <string>
#include <sstream>
#include <exception>
#include <memory>

class AdPlayEventArgs {
public:
    std::string Placement;
    std::string ErrorMessage; // Optional error message for debugging
    std::string EventID;
    std::exception_ptr Exception;

    AdPlayEventArgs(const std::string& placement, const std::string& errorMessage, const std::string& eventID, std::exception_ptr exception = nullptr)
        : Placement(placement), ErrorMessage(errorMessage), EventID(eventID), Exception(exception) {}

    std::string ToString() const {
        std::ostringstream oss;
        oss << "AdPlayEventArgs Placement: " << Placement
            << "; EventID: " << EventID;
        if (!ErrorMessage.empty()) {
            oss << "; Error: " << ErrorMessage;
        }
        return oss.str();
    }
};
