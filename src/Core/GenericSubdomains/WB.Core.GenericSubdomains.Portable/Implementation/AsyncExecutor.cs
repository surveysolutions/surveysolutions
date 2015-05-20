using System;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class AsyncExecutor : IAsyncExecutor
    {
        public void ExecuteAsync(Action action)
        {
            new Task(action).Start();
        }
    }
}