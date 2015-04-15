using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewGroupViewModel : BaseViewModel
    {
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;

        public InterviewGroupViewModel(IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory)
        {
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
        }

        public void Init(string id, string chapterId)
        {
            Items = interviewStateFullViewModelFactory.Load(id, chapterId);
        }

        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }

        public Guid ParentId { get; set; }
        public decimal[] ParentRosterVector { get; set; }

        public string Title { get; set; }
        public string RosterTitle { get; set; }
        public bool IsRoster { get; set; }

        public List<string> Breadcrumbs { get; set; }

        public int CountOfQuestionsWithErrors { get; set; }
        public int CountOfGroupsWithErrors { get; set; }
        public int CountOfAnsweredQuestions { get; set; }
        public int CountOfUnansweredQuestions { get; set; }


        private List<MvxViewModel> items;

        public IEnumerable<MvxViewModel> Items
        {
            get { return items; }
            set { items = new List<MvxViewModel>(value); RaisePropertyChanged(() => Items); }
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}