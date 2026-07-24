using System;
using System.Threading;
using UnityEngine;

namespace VungleAds
{
    public static class VungleThreadDispatcher
    {
        static SynchronizationContext _context;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            _context = SynchronizationContext.Current;
        }

        public static void Enqueue(Action action)
        {
            if (action == null) return;
            _context.Post(_ => action(), null);
        }
    }
}
