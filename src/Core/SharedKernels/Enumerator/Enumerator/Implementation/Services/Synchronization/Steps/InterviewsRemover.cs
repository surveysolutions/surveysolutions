using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class InterviewsRemover : IInterviewsRemover
    {
        private readonly IInterviewerInterviewAccessor interviewFactory;
        private readonly ILogger log;

        public InterviewsRemover(IInterviewerInterviewAccessor interviewFactory,
            ILogger log)
        {
            this.interviewFactory = interviewFactory;
            this.log = log;
        }

        public void RemoveInterviews(SynchronizationStatistics statistics, IProgress<SyncProgressInfo> progress,
            params Guid[] interviewIds)
        {
            statistics.TotalDeletedInterviewsCount += interviewIds.Length;
            foreach (var interviewId in interviewIds)
            {
                this.log.Debug($"Removing interview {interviewId}");
                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_Download_Title,
                    Description = string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                        statistics.DeletedInterviewsCount + 1,
                        interviewIds.Length,
                        EnumeratorUIResources.Synchronization_Interviews),
                    Stage = SyncStage.UpdatingAssignments,
                    StageExtraInfo = new Dictionary<string, string>
                    {
                        { "processedCount", (statistics.DeletedInterviewsCount + 1).ToString() },
                        { "totalCount", interviewIds.Length.ToString()}
                    }
                });

                this.interviewFactory.RemoveInterview(interviewId);
                statistics.DeletedInterviewsCount++;
            }
        }
    }
}
