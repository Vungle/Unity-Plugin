#pragma once
#include <string>
#include <sstream>
#include <exception>
#include <chrono>
#include <iomanip>
#include <ctime>

enum class DiagnosticLogLevel { Trace, Debug, Info, Warn, Error, Fatal };

class DiagnosticLogEvent {
public:
    std::string Message = "";
    std::exception_ptr Exception = nullptr;
    DiagnosticLogLevel Level = DiagnosticLogLevel::Error;
    std::string SenderType = "";


    std::string ToString() const {
        std::ostringstream oss;
        auto now = std::chrono::system_clock::now();
        auto now_c = std::chrono::system_clock::to_time_t(now);
        auto ms = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()) % 1000;

        std::tm localTime;
        localtime_s(&localTime, &now_c);
        oss << std::put_time(&localTime, "%d/%b %H:%M:%S") << '.' << std::setw(3) << std::setfill('0') << ms.count() << ' ';
        oss << LevelToString(Level) << ' ' << SenderType << ' ' << Message << ' ';

        if (Exception) {
            try {
                std::rethrow_exception(Exception);
            } catch (const std::exception& ex) {
                oss << ex.what() << "\r\n";
                for (auto currentException = std::current_exception(); currentException != nullptr; currentException = std::current_exception()) {
                    try {
                        std::rethrow_exception(currentException);
                    } catch (const std::exception& innerEx) {
                        oss << innerEx.what() << "\r\n";
                    }
                }
            }
        }

        return oss.str();
    }
public:
    std::string LevelToString(DiagnosticLogLevel level) const {
        switch (level) {
            case DiagnosticLogLevel::Trace: return "Trace";
            case DiagnosticLogLevel::Debug: return "Debug";
            case DiagnosticLogLevel::Info: return "Info";
            case DiagnosticLogLevel::Warn: return "Warn";
            case DiagnosticLogLevel::Error: return "Error";
            case DiagnosticLogLevel::Fatal: return "Fatal";
            default: return "Unknown";
        }
    }

    bool operator==(const DiagnosticLogEvent& other) const {
        return Message == other.Message &&
            Level == other.Level &&
            SenderType == other.SenderType &&
            // Compare exceptions by their what() message
            (Exception == other.Exception ||
                (Exception && other.Exception &&
                    std::string(what(Exception)) == std::string(what(other.Exception))));
    }

private:
    std::string what(const std::exception_ptr& eptr) const {
        try {
            if (eptr) {
                std::rethrow_exception(eptr);
            }
        }
        catch (const std::exception& ex) {
            return ex.what();
        }
        return "";
    }
};
