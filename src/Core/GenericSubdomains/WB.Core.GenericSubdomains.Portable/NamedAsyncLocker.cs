using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable
{
    /// <summary>
    /// Can be used to lock by key. Details http://johnculviner.com/achieving-named-lock-locker-functionality-in-c-4-0/ 
    /// </summary>
    /// <remarks>
    /// Yes, this class will collect locks and store them in memory for all the time. But TLK is ok with it.
    /// </remarks>
    public class NamedAsyncLocker
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        
        public async Task<TResult> RunWithLockAsync<TResult>(string name, Func<Task<TResult>> body)
        {
            var semaphoreSlim = this.locks.GetOrAdd(name, s => new SemaphoreSlim(1, 1));

            await semaphoreSlim.WaitAsync();
            try
            {
                return await body();
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
        }
    }
}
