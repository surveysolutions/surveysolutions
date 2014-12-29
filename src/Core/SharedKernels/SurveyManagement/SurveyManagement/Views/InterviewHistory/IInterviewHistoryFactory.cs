using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public interface IInterviewHistoryFactory {
        InterviewHistoryView Load(Guid interviewId);
    }
}