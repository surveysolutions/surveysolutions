using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface INetworkService
    {
        bool IsNetworkEnabled();
    }
}
