#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Liftoff.Windows.Editor
{
    /// <summary>
    /// Switches between Debug and Release native plugin DLLs at build time.
    /// Development Build → Debug DLLs, Release Build → Release DLLs.
    /// Also provides menu items for manual switching in the Editor.
    /// </summary>
    public class LiftoffNativePluginConfig : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        static readonly string[] DllNames = {
            "LiftoffSDK.Win32.dll",
            "LiftoffUnityBridge.dll",
            "WebView2Loader.dll"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.StandaloneWindows64)
                return;

            bool isDevelopment = (report.summary.options & BuildOptions.Development) != 0;
            string variant = isDevelopment ? "Debug" : "Release";

            Debug.Log($"[Liftoff] Build preprocessor: activating {variant} native plugins.");
            SetPluginVariant(variant);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.StandaloneWindows64)
                return;

            Debug.Log("[Liftoff] Build postprocessor: restoring Release native plugins as default.");
            SetPluginVariant("Release");
        }

        [MenuItem("Liftoff/Native Plugins/Use Debug")]
        static void UseDebugPlugins()
        {
            SetPluginVariant("Debug");
            Debug.Log("[Liftoff] Switched to Debug native plugins. Editor restart may be required if DLLs are already loaded.");
        }

        [MenuItem("Liftoff/Native Plugins/Use Release")]
        static void UseReleasePlugins()
        {
            SetPluginVariant("Release");
            Debug.Log("[Liftoff] Switched to Release native plugins. Editor restart may be required if DLLs are already loaded.");
        }

        static void SetPluginVariant(string activeVariant)
        {
            var plugins = FindLiftoffPlugins();

            foreach (var entry in plugins)
            {
                string path = entry.Item1;
                PluginImporter importer = entry.Item2;

                string normalized = path.Replace("\\", "/");
                bool isDebug = normalized.Contains("/Debug/");
                bool isRelease = normalized.Contains("/Release/");
                if (!isDebug && !isRelease) continue;

                bool shouldEnable = (activeVariant == "Debug" && isDebug) ||
                                    (activeVariant == "Release" && isRelease);

                bool currentEditor = importer.GetCompatibleWithEditor();
                bool currentWin64 = importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64);

                if (currentEditor != shouldEnable || currentWin64 != shouldEnable)
                {
                    importer.SetCompatibleWithEditor(shouldEnable);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, shouldEnable);
                    importer.SaveAndReimport();
                    Debug.Log($"[Liftoff] {(shouldEnable ? "Enabled" : "Disabled")}: {path}");
                }
            }
        }

        static List<Tuple<string, PluginImporter>> FindLiftoffPlugins()
        {
            var result = new List<Tuple<string, PluginImporter>>();

            foreach (string guid in AssetDatabase.FindAssets("t:PluginImporter"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string normalized = path.Replace("\\", "/");

                if (!normalized.Contains("/Debug/") && !normalized.Contains("/Release/"))
                    continue;

                bool isOurDll = false;
                foreach (string dll in DllNames)
                {
                    if (normalized.EndsWith("/" + dll, StringComparison.OrdinalIgnoreCase))
                    {
                        isOurDll = true;
                        break;
                    }
                }
                if (!isOurDll) continue;

                var importer = AssetImporter.GetAtPath(path) as PluginImporter;
                if (importer != null)
                    result.Add(Tuple.Create(path, importer));
            }

            return result;
        }
    }
}
#endif
