using System;
using System.Threading;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Tracks native handles owned by the selected JavaScript event loop.
    /// </summary>
    public static class ProcessKeepAlive
    {
        private static int _refCount;

        public static bool HasReferences => Volatile.Read(ref _refCount) != 0;

        public static void Acquire() => ChangeReferences(1);

        public static void Release() => ChangeReferences(-1);

        private static void ChangeReferences(int delta)
        {
            while (true)
            {
                var current = Volatile.Read(ref _refCount);
                var next = checked(current + delta);
                if (next < 0)
                    throw new InvalidOperationException("Native event-loop references cannot underflow.");
                if (Interlocked.CompareExchange(ref _refCount, next, current) != current)
                    continue;
                if ((current == 0) != (next == 0)) JsEventLoop.NotifyStateChanged();
                return;
            }
        }
    }
}
