using System.Collections;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IInterviewViewModelFactory
    {
        Task<IEnumerable> GetEntitiesAsync(string interviewId, Identity groupIdentity);
        Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId);
    }
}