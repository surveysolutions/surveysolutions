using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IUserInteractionAwaiter
    {
        Task WaitPendingUserInteractionsAsync();
    }
}