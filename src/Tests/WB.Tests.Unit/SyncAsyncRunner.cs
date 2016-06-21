using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit
{
    public class SyncAsyncRunner : IAsyncRunner
    {
        public void RunAsync(Func<Task> action) => action.Invoke();
    }
}