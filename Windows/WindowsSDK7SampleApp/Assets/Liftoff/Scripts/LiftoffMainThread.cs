using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Liftoff.Windows
{
    public static class LiftoffMainThread
    {
        static SynchronizationContext _ctx;
        static int _mainId;
        static readonly ConcurrentQueue<Action> _fallbackQueue = new ConcurrentQueue<Action>();

        public static bool IsMainThread =>
            Thread.CurrentThread.ManagedThreadId == _mainId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            _ctx = SynchronizationContext.Current;
            _mainId = Thread.CurrentThread.ManagedThreadId;

            // Prevent duplicate dispatchers on Editor domain reloads.
            if (GameObject.Find("[LiftoffDispatcher]") != null) return;

            // Hidden dispatcher drains the fallback queue on every frame,
            // covering the case where SynchronizationContext is unavailable.
            var go = new GameObject("[LiftoffDispatcher]");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Dispatcher>();
        }

        public static void Post(Action a)
        {
            if (a == null) return;
            if (IsMainThread) { a(); return; }

            var ctx = _ctx;
            if (ctx != null)
            {
                ctx.Post(_ => a(), null);
            }
            else
            {
                // SynchronizationContext unavailable (domain reload, etc.)
                // Queue for main-thread dispatch via the Dispatcher's Update().
                _fallbackQueue.Enqueue(a);
            }
        }

        class Dispatcher : MonoBehaviour
        {
            void Update()
            {
                while (_fallbackQueue.TryDequeue(out var action))
                {
                    try { action(); }
                    catch (Exception e) { Debug.LogException(e); }
                }
            }
        }
    }
}
