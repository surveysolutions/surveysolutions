using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public async void Init(string id, string chapterId)
        {
            var viewModels = await interviewStateFullViewModelFactory.LoadAsync(id, chapterId);
            Items = new ObservableCollection<MvxViewModel>(viewModels);
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


        private ObservableCollection<MvxViewModel> items;

        public ObservableCollection<MvxViewModel> Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(() => Items); }
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}