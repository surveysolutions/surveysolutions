using System.Collections.Concurrent;

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
        private readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Get a lock for use with a lock(){} block
        /// </summary>
        public object GetLock(string name)
        {
            return this.locks.GetOrAdd(name, s => new object());
        }
    }
}