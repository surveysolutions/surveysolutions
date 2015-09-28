using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ICapiDataSynchronizationService {
        void ProcessDownloadedInterviewPackages(InterviewSyncPackageDto item, string itemType);
        IList<ChangeLogRecordWithContent> GetItemsToPush();
    }
}