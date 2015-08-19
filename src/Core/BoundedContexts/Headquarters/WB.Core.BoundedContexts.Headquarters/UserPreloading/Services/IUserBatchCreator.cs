namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal interface IUserBatchCreator
    {
        void CreateUsersFromReadyToBeCreatedQueue();
    }
}