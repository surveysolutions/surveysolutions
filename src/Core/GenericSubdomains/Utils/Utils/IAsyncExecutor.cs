using System;

namespace WB.Core.GenericSubdomains.Utils
{
    public interface IAsyncExecutor
    {
        void ExecuteAsync(Action action);
    }
}