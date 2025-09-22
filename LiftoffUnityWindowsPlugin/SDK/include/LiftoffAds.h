#pragma once  
#include <windows.h>

#include <string>  
#include <memory>  
#include <future>  
#include <functional>  
#include "EventArguments/InitializationSuccessEventArgs.h"
#include "EventArguments/InitializationFailureEventArgs.h"  
#include "EventArguments/AdLoadEventArgs.h"
#include "Configuration/LiftoffAdPlayInfo.h"  
#include "Configuration/AdConfig.h"  
#include "EventArguments/AdLoadCallback.h"
#include "EventArguments/AdPlayCallback.h"
#include "EventArguments/InitializationCallback.h"
#include "EventArguments/DiagnosticLogEvent.h"

// CCPA consent status
enum class CcpaConsentStatus {
    /// <summary>
    /// "OptedIn" means that the user has given consent to share their data with Liftoff
    /// </summary>
    OptedIn = 1,
    /// <summary>
    /// "OptedOut" means that the user has not given consent to share their data with Liftoff
    /// </summary>
    OptedOut = 2
};

// GDPR consent status
enum class GdprConsentStatus {
    /// <summary>
    /// User is allowing Liftoff to collect data on them
    /// </summary>
    ConsentAccepted = 1,
    /// <summary>
    /// User is not allowing Liftoff to collect data on them
    /// </summary>
    ConsentDenied = 2
};

// include this _after_ enum declarations above
#include "Configuration/LiftoffSdkConfig.h"

class __declspec(dllexport) LiftoffAds {  
#if DONT_WANT
private:  
   static std::unique_ptr<LiftoffAds> m_instance;  
   static std::unique_ptr<InstanceController> m_instanceController;  
#endif
public:  

   static std::future<LiftoffAds*> InitializeAsync(  
       const std::string& appID,  
       HWND hWnd,  
	   const InitializationCallback& initCallback);

   static std::future<LiftoffAds*> InitializeAsync(  
       const std::string& appID,  
       const LiftoffSdkConfig& config,  
       HWND hWnd,  
       const InitializationCallback& initCallback);  

   bool LoadAd(const std::string& placement,
       const AdLoadCallback callback);
   LiftoffAdPlayInfo PlayAd(const std::string& placement, const AdPlayCallback callbacks, const AdConfig config);
   LiftoffAdPlayInfo PlayAd(const std::string& placement, const AdPlayCallback callbacks);
   bool IsAdPlayable(const std::string& placement); // Add this declaration
   static void AddDiagnosticListener(std::function<void(const DiagnosticLogEvent)> listener);  
   static void RemoveDiagnosticListener(std::function<void(const DiagnosticLogEvent)> listener);  
   static bool GetCoppaStatus();
   static void SetCoppaStatus(bool status);
   static CcpaConsentStatus GetCcpaStatus();
   static void SetCcpaStatus(CcpaConsentStatus status);

   static GdprConsentStatus GetGdprConsentStatus();
   static void SetGdprConsentStatus(GdprConsentStatus status, const std::string& version);
   static std::string GetGdprConsentMessageVersion();
   static void ResetGdprConsentStatusToUnknown();
};
