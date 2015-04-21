using System;
using System.Collections;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewGroupViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        public InterviewGroupViewModel(IPrincipal principal, IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory,
             IPlainRepository<QuestionnaireModel> questionnaireRepository,
             IPlainRepository<InterviewModel> interviewRepository,
             InterviewLeftSidePanelViewModel interviewLeftSidePanelViewModel)
        {
            this.principal = principal;
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.LeftSide = interviewLeftSidePanelViewModel;
        }

        public async void Init(string id, string chapterId)
        {
            var interview = this.interviewRepository.Get(id);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId.FormatGuid());

            var groupId = string.IsNullOrEmpty(chapterId) 
                ? questionnaire.GroupsWithoutNestedChildren.Keys.First()
                : Guid.Parse(chapterId);

            var group = questionnaire.GroupsWithoutNestedChildren[groupId];

            this.LeftSide.Init(id, chapterId);

            this.CurrentGroupName = group.Title;
            this.Items = await this.interviewStateFullViewModelFactory.LoadAsync(id, chapterId);
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