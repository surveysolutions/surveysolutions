using System;
using WB.Core.BoundedContexts.Headquarters.Troubleshooting.Views;

namespace WB.Core.BoundedContexts.Headquarters.Troubleshooting
{
    public interface IInterviewLogSummaryReader
    {
        InterviewSyncLogSummary GetInterviewLog(Guid interviewId, Guid responsibleId);
    }
}