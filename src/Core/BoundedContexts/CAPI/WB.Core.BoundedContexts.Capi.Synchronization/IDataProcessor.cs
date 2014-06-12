using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.BoundedContexts.Capi.Synchronization
{
    public interface IDataProcessor {
        void ProcessPulledItem(SyncItem item);
        IList<ChangeLogRecordWithContent> GetItemsForPush();
    }
}