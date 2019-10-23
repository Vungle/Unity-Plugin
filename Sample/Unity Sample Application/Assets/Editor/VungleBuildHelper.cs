using System;
using UnityEditor;
using UnityEngine;

class VungleBuildHelper
{
#if UNITY_5 || UNITY_2017
    [UnityEditor.MenuItem("Tools/Vungle/Prepare Windows 10 Build")]
    static void prepareWin10()
    {
        PluginImporter pi = (PluginImporter)PluginImporter.GetAtPath("Assets/Plugins/Metro/VungleSDKProxy.winmd");
        pi.SetPlatformData(BuildTarget.WSAPlayer, "PlaceholderPath", "Assets/Plugins/VungleSDKProxy.dll");
        pi.SaveAndReimport();
        pi = (PluginImporter)PluginImporter.GetAtPath("Assets/Plugins/Metro/VungleSDK.winmd");
        pi.SetPlatformData(BuildTarget.WSAPlayer, "SDK", "SDK81");
        pi = (PluginImporter)PluginImporter.GetAtPath("Assets/Plugins/Metro/UWP/VungleSDK.winmd");
        pi.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, true);
        pi.SetPlatformData(BuildTarget.WSAPlayer, "SDK", "UWP");
        pi.SaveAndReimport();
    }

    [UnityEditor.MenuItem("Tools/Vungle/Prepare Windows 8.1 Build")]
    static void prepareWin81()
    {
        PluginImporter pi = (PluginImporter)PluginImporter.GetAtPath("Assets/Plugins/Metro/VungleSDKProxy.winmd");
        pi.SetPlatformData(BuildTarget.WSAPlayer, "PlaceholderPath", "Assets/Plugins/VungleSDKProxy.dll");
        pi.SaveAndReimport();
        pi = (PluginImporter)PluginImporter.GetAtPath("Assets/Plugins/Metro/VungleSDK.winmd");
        pi.SetPlatformData(BuildTarget.WSAPlayer, "SDK", "SDK81");
        pi.SaveAndReimport();
        pi = (PluginImporter)PluginImporter.GetAtPath("Assets/Plugins/Metro/UWP/VungleSDK.winmd");
        pi.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
        pi.SaveAndReimport();
    }
#endif
}