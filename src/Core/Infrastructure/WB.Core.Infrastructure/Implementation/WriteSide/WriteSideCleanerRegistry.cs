using System.Collections.Generic;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure.Implementation.WriteSide
{
    internal class WriteSideCleanerRegistry : IWriteSideCleanerRegistry
    {
        private readonly List<IWriteSideCleaner> cleaners = new List<IWriteSideCleaner>();

        public void Register(IWriteSideCleaner writeSideCleaner)
        {
            this.cleaners.Add(writeSideCleaner);
        }

        public IEnumerable<IWriteSideCleaner> GetAll()
        {
            return cleaners;
        }
    }
}