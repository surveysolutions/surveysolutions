using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ICapiDataSynchronizationService {
        void ProcessDownloadedPackage(UserSyncPackageDto item);
        void ProcessDownloadedPackage(QuestionnaireSyncPackageDto item);
        void ProcessDownloadedPackage(InterviewSyncPackageDto item);
        IList<ChangeLogRecordWithContent> GetItemsForPush();
    }
}