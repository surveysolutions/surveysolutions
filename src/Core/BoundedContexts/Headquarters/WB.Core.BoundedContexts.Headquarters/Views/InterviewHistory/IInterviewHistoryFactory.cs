using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public interface IInterviewHistoryFactory {
        InterviewHistoryView Load(Guid interviewId);
        InterviewHistoryView[] Load(Guid[] interviewIds);
    }
}
