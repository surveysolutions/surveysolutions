namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal interface IUserPreloadingVerifier
    {
        void VerifyProcessFromReadyToBeVerifiedQueue();
    }
}