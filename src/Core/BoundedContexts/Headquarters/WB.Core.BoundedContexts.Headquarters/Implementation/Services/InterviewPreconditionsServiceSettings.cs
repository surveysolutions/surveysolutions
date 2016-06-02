namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class InterviewPreconditionsServiceSettings
    {
        public InterviewPreconditionsServiceSettings(int? interviewLimitCount)
        {
            this.InterviewLimitCount = interviewLimitCount;
        }

        public int? InterviewLimitCount { get; private set; }
    }
}