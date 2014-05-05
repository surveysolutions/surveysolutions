namespace WB.UI.Supervisor.Code
{
    public interface IRevalidateInterviewsAdministrationService
    {
        void RevalidateAllInterviewsWithErrorsAsync();

        string GetReadableStatus();

        void StopInterviewsRevalidating();

    }
}
