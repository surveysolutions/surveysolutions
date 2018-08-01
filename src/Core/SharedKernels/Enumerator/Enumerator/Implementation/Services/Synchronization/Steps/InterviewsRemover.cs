using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class InterviewsRemover : IInterviewsRemover
    {
        private readonly IInterviewerInterviewAccessor interviewFactory;

        public InterviewsRemover(IInterviewerInterviewAccessor interviewFactory)
        {
            this.interviewFactory = interviewFactory;
        }

        public void RemoveInterviews(List<Guid> interviewIds, SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress)
        {
            statistics.TotalDeletedInterviewsCount += interviewIds.Count;
            foreach (var interviewId in interviewIds)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        statistics.DeletedInterviewsCount + 1,
                        interviewIds.Count,
                        InterviewerUIResources.Synchronization_Interviews)
                });

                this.interviewFactory.RemoveInterview(interviewId);
                statistics.DeletedInterviewsCount++;
            }
        }
    }
}
