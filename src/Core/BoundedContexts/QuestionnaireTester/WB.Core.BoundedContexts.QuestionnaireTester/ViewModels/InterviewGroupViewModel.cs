using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewGroupViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;

        public InterviewGroupViewModel(IPrincipal principal, IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory)
        {
            this.principal = principal;
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
        }

        public async void Init(string id, string chapterId)
        {
            this.CurrentGroupName = "Current group name";
            this.Items = await interviewStateFullViewModelFactory.LoadAsync(id, chapterId);
        }

        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get
            {
                return navigateToDashboardCommand ?? (navigateToDashboardCommand = new MvxCommand(() => this.ShowViewModel<DashboardViewModel>()));
            }
        }

        private IMvxCommand navigateToHelpCommand;
        public IMvxCommand NavigateToHelpCommand
        {
            get
            {
                return navigateToHelpCommand ?? (navigateToHelpCommand = new MvxCommand(() => this.ShowViewModel<HelpViewModel>()));
            }
        }

        private IMvxCommand changeLanguageCommand;
        public IMvxCommand ChangeLanguageCommand
        {
            get
            {
                return changeLanguageCommand ?? (changeLanguageCommand = new MvxCommand(this.ChangeLanguage));
            }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return signOutCommand ?? (signOutCommand = new MvxCommand(this.SignOut)); }
        }

        private IMvxCommand showSettingsCommand;
        public IMvxCommand ShowSettingsCommand
        {
            get { return showSettingsCommand ?? (showSettingsCommand = new MvxCommand(() => this.ShowViewModel<SettingsViewModel>())); }
        }

        private void SignOut()
        {
            this.principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
        }

        private void ChangeLanguage()
        {
            throw new NotImplementedException();
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

        public string CurrentGroupName { get; set; }


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