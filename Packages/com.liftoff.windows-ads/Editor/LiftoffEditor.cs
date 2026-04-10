// LiftoffEditor.cs (Editor-only teardown hook)
#if UNITY_EDITOR
using UnityEditor;

namespace Liftoff.Windows
{
    [InitializeOnLoad]
    static class LiftoffEditorTeardown
    {
        static bool _subscribed;

        static LiftoffEditorTeardown()
        {
            if (_subscribed) return;
            _subscribed = true;

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        static void OnPlayModeChanged(PlayModeStateChange s)
        {
            if (s == PlayModeStateChange.ExitingPlayMode)
                LiftoffWindows.Shutdown();
        }

        static void OnBeforeAssemblyReload()
        {
            LiftoffWindows.Shutdown();
        }
    }
}
#endif
