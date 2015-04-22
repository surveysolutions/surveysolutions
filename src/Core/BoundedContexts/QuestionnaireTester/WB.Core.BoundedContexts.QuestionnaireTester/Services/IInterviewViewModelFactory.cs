using System.Collections;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IInterviewViewModelFactory
    {
        Task<IEnumerable> LoadAsync(string interviewId, string chapterId);
        Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId);
    }
}