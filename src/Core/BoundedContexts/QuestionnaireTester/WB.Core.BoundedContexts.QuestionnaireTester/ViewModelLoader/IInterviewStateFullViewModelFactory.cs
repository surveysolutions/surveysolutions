using System.Collections;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader
{
    public interface IInterviewStateFullViewModelFactory
    {
        Task<IEnumerable> LoadAsync(string interviewId, string chapterId);
        Task<IEnumerable> GetPrefilledQuestionsAsync(string interviewId);
    }
}