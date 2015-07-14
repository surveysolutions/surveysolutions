using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IUserInteractionAwaiter
    {
        Task WaitPendingUserInteractionsAsync();
    }
}