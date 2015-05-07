namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public interface IInterviewersViewFactory
    {
        InterviewersView Load(InterviewersInputModel input);
    }
}