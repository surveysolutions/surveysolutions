using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader
{
    public interface IInterviewStateFullViewModelFactory
    {
        Task<List<MvxViewModel>> LoadAsync(string interviewId, string chapterId);
        Task<ObservableCollection<MvxViewModel>> GetPrefilledQuestionsAsync(string interviewId);
    }
}