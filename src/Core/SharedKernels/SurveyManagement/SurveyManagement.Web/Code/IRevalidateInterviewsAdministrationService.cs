namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public interface IRevalidateInterviewsAdministrationService
    {
        void RevalidateAllInterviewsWithErrorsAsync();

        string GetReadableStatus();

        void StopInterviewsRevalidating();

    }
}
