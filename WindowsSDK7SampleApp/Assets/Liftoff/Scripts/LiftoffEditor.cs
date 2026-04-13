// LiftoffEditor.cs (Editor-only teardown hook)
#if UNITY_EDITOR
using UnityEditor;

namespace Liftoff.Windows
{
    [InitializeOnLoad]
    static class LiftoffEditorTeardown
    {
        static LiftoffEditorTeardown()
        {
            EditorApplication.playModeStateChanged += s =>
            {
                if (s == PlayModeStateChange.ExitingPlayMode)
                    LiftoffWindows.Shutdown();
            };
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                LiftoffWindows.Shutdown();
            };
        }
    }
}
#endif
