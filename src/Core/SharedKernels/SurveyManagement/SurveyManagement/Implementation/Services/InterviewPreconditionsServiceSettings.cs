namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class InterviewPreconditionsServiceSettings
    {
        public InterviewPreconditionsServiceSettings(int? interviewLimitCount)
        {
            InterviewLimitCount = interviewLimitCount;
        }

        public int? InterviewLimitCount { get; private set; }
    }
}