namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ILimitedInterviewService
    {
        long Limit { get; }

        long CreatedInterviewCount { get; }
    }
}