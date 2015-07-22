using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public interface IAsyncExecutor
    {
        void ExecuteAsync(Action action);
    }
}