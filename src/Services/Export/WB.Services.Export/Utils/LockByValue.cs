using System;
using System.Collections.Concurrent;

namespace WB.Services.Export
{
    public class LockByValue<T> where T : notnull
    {
        private static readonly ConcurrentDictionary<T, object> locks = new ConcurrentDictionary<T, object>();

        public static void Lock(T value, Action action)
        {
            lock (locks.GetOrAdd(value, new object()))
            {
                action.Invoke();
            }
        }
    }
}
