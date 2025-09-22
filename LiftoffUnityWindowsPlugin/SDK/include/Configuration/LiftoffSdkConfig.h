#pragma once


#include <string>

class LiftoffSdkConfig {
public:
    friend class LiftoffAds;

    // Gets or sets api endpoint URL
    std::string ApiEndpoint;

    // The "ashwid" is the Application Specific Hardware ID
    bool DisableAshwidTracking = false;

    void SetCoppaStatus(bool consent) {
        CoppaStatusValue = consent;
        HasCoppaStatus = true;
	}

    void SetCcpaConsentStatus(CcpaConsentStatus consent) {
        CcpaConsentStatusValue = consent;
        HasCcpaConsentStatus = true;
	}


private:
    // Setting CoppaStatus to true will prevent the SDK from collecting any data on the user
    bool CoppaStatusValue = false;
    bool HasCoppaStatus = false;

    // Set the CCPA status to OptedIn if you want to allow the SDK to collect data on the user (the default)
    // Set the CCPA status to OptedOut if you want to prevent the SDK from collecting data on the user
    CcpaConsentStatus CcpaConsentStatusValue = CcpaConsentStatus::OptedIn; // Ensure CcpaConsentStatus is defined
    bool HasCcpaConsentStatus = false;

};
