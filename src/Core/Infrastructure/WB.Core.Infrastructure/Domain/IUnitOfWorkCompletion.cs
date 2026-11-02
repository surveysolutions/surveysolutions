using System;

namespace WB.Core.Infrastructure.Domain
{
    public interface IUnitOfWorkCompletion
    {
        void OnCompleted(Action<bool> action);
    }
}
