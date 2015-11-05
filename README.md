# Vungle's Unity-Plugin

## Getting Started
To get up and running with Vungle, you'll need to [Create an Account With Vungle](https://v.vungle.com/dashboard/signup) and [Add an Application to the Vungle Dashboard](https://support.vungle.com/hc/en-us/articles/204249614-Adding-an-Application-to-the-Vungle-Dashboard)

Once you've created an account you can follow our [Getting Started for Unity Guide](https://support.vungle.com/hc/en-us/articles/204311244-Get-Started-with-Vungle-Unity-Combo-) to complete the integration. Remember to get the Vungle App ID from the Vungle dashboard.

### Requirements
* The Vungle Unity Plugin supports both Unity 5 and Unity 4.

## Release Notes
## VERSION 2.2.4
* Integrated Android Publisher SDK v3.3.3
* Resolved potential package conflict related to MiniJSON libary
* Implemented OnAdPlayableEvent to sync up with platform SDK
* Implemented OnAdFinishedEvent that replaces deprecated onAdEndedEvent and
onAdViewedEvent. New event provide single source of information for ad completion.
* Added VersionInfo property to query plug-in and SDK version for diagnostic purposes
* Fixed several minor bugs

## License
The Vungle iOS-SDK is available under a commercial license. See the LICENSE file for more info.
