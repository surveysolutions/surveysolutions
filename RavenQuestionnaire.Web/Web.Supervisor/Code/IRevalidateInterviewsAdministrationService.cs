namespace Web.Supervisor.Code
{
    public interface IRevalidateInterviewsAdministrationService
    {
        void RevalidateAllInterviewsWithErrorsAsync();

        string GetReadableStatus();

        void StopInterviewsRevalidating();

    }
}
