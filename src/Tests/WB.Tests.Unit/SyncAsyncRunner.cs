using System;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit
{
    public class SyncAsyncRunner : IAsyncRunner
    {
        public void RunAsync(Func<Task> runTask) => runTask().WaitAndUnwrapException();
    }
}