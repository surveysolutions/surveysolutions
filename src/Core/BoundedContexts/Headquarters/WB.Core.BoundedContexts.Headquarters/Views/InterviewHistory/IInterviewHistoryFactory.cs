using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public interface IInterviewHistoryFactory {
        InterviewHistoryView Load(Guid interviewId, bool? reduced);
        InterviewHistoryView[] Load(Guid[] interviewIds, bool? reduced);
    }
}
