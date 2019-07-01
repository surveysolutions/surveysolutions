﻿using System;
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

        public void RemoveInterviews(SynchronizationStatistics statistics, IProgress<SyncProgressInfo> progress,
            params Guid[] interviewIds)
        {
            statistics.TotalDeletedInterviewsCount += interviewIds.Length;
            foreach (var interviewId in interviewIds)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_Download_Title,
                    Description = string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                        statistics.DeletedInterviewsCount + 1,
                        interviewIds.Length,
                        EnumeratorUIResources.Synchronization_Interviews),
                    Stage = SyncStage.UpdatingAssignments,
                    StageExtraInfo = new Dictionary<string, string>()
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
