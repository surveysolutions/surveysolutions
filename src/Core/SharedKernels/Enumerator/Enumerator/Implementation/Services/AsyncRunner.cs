using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AsyncRunner : IAsyncRunner
    {
        public void RunAsync(Func<Task> runTask) => runTask.Invoke();
    }
}
