namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewSummaryViewFactory
    {
        InterviewSummary Load(string interviewId);
    }
}