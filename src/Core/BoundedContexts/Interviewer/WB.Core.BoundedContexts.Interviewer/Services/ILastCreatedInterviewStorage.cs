namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ILastCreatedInterviewStorage
    {
        void Store(string interviewId);

        bool WasJustCreated(string interviewId);
    }
}