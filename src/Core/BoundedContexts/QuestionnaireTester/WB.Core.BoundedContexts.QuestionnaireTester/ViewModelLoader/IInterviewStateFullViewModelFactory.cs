using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader
{
    public interface IInterviewStateFullViewModelFactory
    {
        Task<ObservableCollection<MvxViewModel>> LoadAsync(string interviewId, string chapterId);
        List<MvxViewModel> GetPrefilledQuestionsAsync(string interviewId);
    }
}