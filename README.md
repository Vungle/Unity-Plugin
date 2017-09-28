# Vungle's Unity-Plugin

## Getting Started
To get up and running with Vungle, you'll need to [Create an Account With Vungle](https://v.vungle.com/dashboard) and [Add an Application to the Vungle Dashboard](https://support.vungle.com/hc/en-us/articles/210468678)

Once you've created an account you can follow our [Getting Started for Unity Guide](https://support.vungle.com/hc/en-us/articles/204311244-Get-Started-with-Vungle-Unity-Combo-) to complete the integration. Remember to get the Vungle App ID from the Vungle dashboard.

### Requirements
* The Vungle Unity Plugin supports both Unity 5 and Unity 4.
* Windows 10 supports for Unity 5.2 and higher.

### Running the Vungle sample app
To run our sample app, download our Unity Sample app.  Create a new project in Unity.  With Unity open and your project presented, double-click the downloaded VunglePlugin.unitypackage file to add the Vungle Unity Plugin to your application.

Click All to select everything before importing.

In Project window, navigate to the Assets Folder, check to see if this folder has all the files in your downloaded project Assets folder.  If all these files are not in your Unity Assets folder then move them manually by right clicking on Assets folder in Unity and choose "open in finder", in the opened finder window copy and paste everything from the downloaded Sample app assets folder to your Unity project's Asset folder and replace the files if necessary.

In Unity, project navigator->Assets doubleclick on MainTitleScreen.

Click on GameObject, choose TitleGUI inside Inspector->Title GUI (Script).

Press Command + Shift + B to open up Build Settings.  Click on iOS or Android then hit Switch Platform.

In the Build Settings window click on Player Settings.  In Inspector, make sure Company Name, Product Name, Package Name are your own and correct values.

## Release Notes

#### VERSION 5.3.0
* Integrated iOS Publisher SDK v5.3.0
* Integrated Android Publisher SDK v5.3.0

#### VERSION 5.2.1
* Integrated iOS Publisher SDK v5.2.0

## License
The Vungle Unity-Plugin is available under a commercial license. See the LICENSE file for more info.
