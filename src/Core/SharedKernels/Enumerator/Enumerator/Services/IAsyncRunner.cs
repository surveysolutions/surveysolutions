using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAsyncRunner
    {
        void RunAsync(Func<Task> runTask);
    }
}
