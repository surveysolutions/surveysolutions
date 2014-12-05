using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ICapiDataSynchronizationService {
        void SavePulledItem(SyncItem item);
        IList<ChangeLogRecordWithContent> GetItemsForPush();
    }
}