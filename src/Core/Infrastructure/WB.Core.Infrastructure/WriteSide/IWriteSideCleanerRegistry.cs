using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.WriteSide
{
    public interface IWriteSideCleanerRegistry
    {
        void Register(IWriteSideCleaner writeSideCleaner);
        IEnumerable<IWriteSideCleaner> GetAll();
    }
}