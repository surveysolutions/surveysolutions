using System;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class AsyncExecutor : IAsyncExecutor
    {
        public void ExecuteAsync(Action action)
        {
            new Task(action).Start();
        }
    }
}