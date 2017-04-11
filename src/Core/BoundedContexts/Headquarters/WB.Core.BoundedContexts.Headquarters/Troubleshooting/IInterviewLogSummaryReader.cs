using System;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;

namespace WB.Core.BoundedContexts.Headquarters.Troubleshooting
{
    public interface IInterviewLogSummaryReader
    {
        InterviewLog GetInterviewLog(Guid interviewId, Guid responsibleId);
    }
}