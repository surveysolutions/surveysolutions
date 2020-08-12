using System;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateLock
    {
        T RunWithLock<T>(string aggregateGuid, Func<T> run);
        void RunWithLock(string aggregateGuid, Action run);
    }
}
