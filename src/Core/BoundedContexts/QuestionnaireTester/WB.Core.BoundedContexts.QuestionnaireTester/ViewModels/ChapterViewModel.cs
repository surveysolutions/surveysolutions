using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class ChapterViewModel : BaseViewModel
    {
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;

        public ChapterViewModel(IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory)
        {
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
        }

        public void Init(string interviewId, string chapterId)
        {
            Entities = interviewStateFullViewModelFactory.Load(interviewId, chapterId);
        }

        private List<object> entities;

        public IEnumerable<object> Entities
        {
            get { return entities; }
            set { entities = new List<object>(value); RaisePropertyChanged(() => Entities); }
        }

        public override void NavigateToPreviousViewModel()
        {

        }
    }
}