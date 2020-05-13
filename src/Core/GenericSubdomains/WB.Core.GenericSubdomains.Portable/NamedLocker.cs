using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WB.Core.GenericSubdomains.Portable
{
    /// <summary>
    /// Can be used to lock by key. Details http://johnculviner.com/achieving-named-lock-locker-functionality-in-c-4-0/ 
    /// </summary>
    /// <remarks>
    /// Yes, this class will collect locks and store them in memory for all the time. But TLK is ok with it.
    /// </remarks>
    public class NamedLocker
    {
        private readonly ConcurrentDictionary<string, KeyHolder> locks = new ConcurrentDictionary<string, KeyHolder>();

        private class KeyHolder
        {
            public long HoldersCount = 0;
        }

        internal int LocksCount => locks.Count;
        
        public TResult RunWithLock<TResult>(string name, Func<TResult> body)
        {
            var lockKey = this.locks.GetOrAdd(name, s => new KeyHolder());
            try
            {
                Interlocked.Increment(ref lockKey.HoldersCount);
                lock (lockKey)
                {
                    return body();
                }
            }
            finally
            {
                var value = Interlocked.Decrement(ref lockKey.HoldersCount);
                if (value <= 0)
                {
                    this.locks.TryRemove(name, out _);
                }
            }
        }

        public void RunWithLock(string name, Action body)
        {
            var lockKey = this.locks.GetOrAdd(name, s => new KeyHolder());
            try
            {
                Interlocked.Increment(ref lockKey.HoldersCount);
                lock (lockKey)
                {
                    body();
                }
            }
            finally
            {
                var value = Interlocked.Decrement(ref lockKey.HoldersCount);
                if (value <= 0)
                {
                    this.locks.TryRemove(name, out _);
                }
            }
        }
    }
}
