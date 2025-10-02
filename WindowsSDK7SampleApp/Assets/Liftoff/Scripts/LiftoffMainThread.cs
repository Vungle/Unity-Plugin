using System;
using System.Threading;
using UnityEngine;

namespace Liftoff.Windows
{
    public static class LiftoffMainThread
    {
        static SynchronizationContext _ctx;
        static int _mainThreadId;

        public static bool IsMainThread =>
            Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            // Runs on the Unity main thread when you press Play.
            _ctx = SynchronizationContext.Current;  // UnitySynchronizationContext
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>Queue work to run on the Unity main thread (next frame).</summary>
        public static void Post(Action action)
        {
            if (action == null) return;

            if (IsMainThread) { action(); return; }      // already on main thread
            var ctx = _ctx;                               // local copy for thread-safety
            if (ctx != null) ctx.Post(_ => action(), null);
            else
            {
                // Fallback: if called very early before Boot(), just run inline.
                // (Boot() will run before any scene content executes.)
                action();
            }
        }

        /// <summary>
        /// Run on main thread synchronously. Avoid calling this from the main thread itself
        /// or you’ll deadlock; prefer Post() in most cases.
        /// </summary>
        public static void Send(Action action)
        {
            if (action == null) return;

            if (IsMainThread) { action(); return; }
            var ctx = _ctx;
            if (ctx != null) ctx.Send(_ => action(), null);
            else action();
        }
    }
}
