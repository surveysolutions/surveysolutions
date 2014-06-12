namespace WB.UI.Headquarters.Code
{
    public interface IRevalidateInterviewsAdministrationService
    {
        void RevalidateAllInterviewsWithErrorsAsync();

        string GetReadableStatus();

        void StopInterviewsRevalidating();

    }
}
