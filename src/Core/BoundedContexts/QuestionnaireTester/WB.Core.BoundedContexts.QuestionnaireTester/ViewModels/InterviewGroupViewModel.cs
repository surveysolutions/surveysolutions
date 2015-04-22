using System;
using System.Collections;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewGroupViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        public InterviewGroupViewModel(IPrincipal principal, IInterviewViewModelFactory interviewViewModelFactory,
             IPlainRepository<QuestionnaireModel> questionnaireRepository,
             IPlainRepository<InterviewModel> interviewRepository,
             InterviewLeftSidePanelViewModel interviewLeftSidePanelViewModel)
        {
            this.principal = principal;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.LeftSide = interviewLeftSidePanelViewModel;
        }

        public async void Init(string id, Identity chapterIdentity)
        {
            var interview = this.interviewRepository.Get(id);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            if (chapterIdentity == null)
            {
                chapterIdentity = new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]);
            }

            var group = questionnaire.GroupsWithoutNestedChildren[chapterIdentity.Id];

            this.LeftSide.Init(id, chapterIdentity);

            this.CurrentGroupName = group.Title;
            this.Items = await this.interviewViewModelFactory.GetEntitiesAsync(id, chapterIdentity);
        }

        public InterviewLeftSidePanelViewModel LeftSide { get; set; }
        public string CurrentGroupName { get; set; }

        private IEnumerable items;
        public IEnumerable Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(() => Items); }
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

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}