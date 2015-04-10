using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewGroupViewModel : BaseViewModel
    {
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;

        public InterviewGroupViewModel(IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory)
        {
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
        }

        public void Init(string interviewId, string chapterId)
        {
            Entities = interviewStateFullViewModelFactory.Load(interviewId, chapterId);
        }

        private List<MvxViewModel> entities;

        public IEnumerable<MvxViewModel> Entities
        {
            get { return entities; }
            set { entities = new List<MvxViewModel>(value); RaisePropertyChanged(() => Entities); }
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}