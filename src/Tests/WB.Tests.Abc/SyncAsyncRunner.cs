using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Abc
{
    public class SyncAsyncRunner : IAsyncRunner
    {
        public void RunAsync(Func<Task> runTask) => runTask().WaitAndUnwrapException();
    }
}