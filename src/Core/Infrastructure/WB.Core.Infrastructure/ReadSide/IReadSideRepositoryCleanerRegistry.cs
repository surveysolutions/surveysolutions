using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.ReadSide
{
    public interface IReadSideRepositoryCleanerRegistry
    {
        void Register(IReadSideRepositoryCleaner writer);

        IEnumerable<IReadSideRepositoryCleaner> GetAll();
    }
}
