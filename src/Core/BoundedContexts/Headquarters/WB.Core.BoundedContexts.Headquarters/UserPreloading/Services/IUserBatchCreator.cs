namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserBatchCreator
    {
        void CreateUsersFromReadyToBeCreatedQueue();
    }
}