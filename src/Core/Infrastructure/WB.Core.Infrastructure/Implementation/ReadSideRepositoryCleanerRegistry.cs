using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Implementation
{
    public class ReadSideRepositoryCleanerRegistry : IReadSideRepositoryCleanerRegistry
    {
        private readonly List<IReadSideRepositoryCleaner> cleaners = new List<IReadSideRepositoryCleaner>();

        public void Register(IReadSideRepositoryCleaner cleaner)
        {
            this.cleaners.Add(cleaner);
        }

        public IEnumerable<IReadSideRepositoryCleaner> GetAll()
        {
            return this.cleaners;
        }
    }
}
