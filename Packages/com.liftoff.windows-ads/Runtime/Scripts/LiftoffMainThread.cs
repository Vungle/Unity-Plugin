using System;
using System.Threading;
using UnityEngine;

namespace Liftoff.Windows
{
    public static class LiftoffMainThread
    {
        static System.Threading.SynchronizationContext _ctx;
        static int _mainId;

        public static bool IsMainThread =>
            System.Threading.Thread.CurrentThread.ManagedThreadId == _mainId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            _ctx = System.Threading.SynchronizationContext.Current;
            _mainId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static void Post(Action a)
        {
            if (a == null) return;
            if (IsMainThread) { a(); return; }
            var ctx = _ctx; if (ctx != null) ctx.Post(_ => a(), null); else a();
        }
    }
}
