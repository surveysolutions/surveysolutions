using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserBatchCreator
    {
        Task CreateUsersFromReadyToBeCreatedQueueAsync();
    }
}