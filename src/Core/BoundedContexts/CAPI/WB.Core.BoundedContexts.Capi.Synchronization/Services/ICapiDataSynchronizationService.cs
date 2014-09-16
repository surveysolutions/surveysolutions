
using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Services
{
    public interface ICapiDataSynchronizationService {
        void SavePulledItem(SyncItem item);
        IList<ChangeLogRecordWithContent> GetItemsForPush();
    }
}