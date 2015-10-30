namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserPreloadingVerifier
    {
        void VerifyProcessFromReadyToBeVerifiedQueue();
    }
}