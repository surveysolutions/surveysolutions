using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class AggregateLock : IAggregateLock
    {
        private readonly NamedLocker locker = new NamedLocker();
        
        public T RunWithLock<T>(string aggregateGuid, Func<T> run)
        {
            return this.locker.RunWithLock(aggregateGuid, run);
        }

        public void RunWithLock(string aggregateGuid, Action run)
        {
            this.locker.RunWithLock(aggregateGuid, run);
        }
    }
}
