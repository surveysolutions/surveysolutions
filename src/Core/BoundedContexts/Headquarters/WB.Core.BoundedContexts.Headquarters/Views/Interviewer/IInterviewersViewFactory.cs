namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public interface IInterviewersViewFactory
    {
        InterviewersView Load(InterviewersInputModel input);
    }
}