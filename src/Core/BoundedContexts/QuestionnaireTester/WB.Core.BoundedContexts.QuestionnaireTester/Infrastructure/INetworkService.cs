using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface INetworkService
    {
        Task<bool> IsNetworkEnabledAsync();
    }
}
