using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface IIncomePackagesRepository
    {
        void StoreIncomingItem(SyncItem item);
        IEnumerable<Guid> GetListOfUnhandledPackages();
        string GetUnhandledPackagePath(Guid id);
    }
}  