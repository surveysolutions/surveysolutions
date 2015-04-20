using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewGroupViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainRepository<InterviewModel> plainStorageInterviewAccessor;

        public InterviewGroupViewModel(IPrincipal principal, IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory,
             IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IPlainRepository<InterviewModel> plainStorageInterviewAccessor)
        {
            this.principal = principal;
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
        }

        public async void Init(string id, string chapterId)
        {
            var interview = this.plainStorageInterviewAccessor.Get(id);
            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);

            this.CurrentGroupName = string.IsNullOrEmpty(chapterId)? ((IGroup)questionnaire.Children[0]).Title : questionnaire.Find<IGroup>(Guid.Parse(chapterId)).Title;
            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.GetFeaturedQuestions()
                .Select(_ => new PrefilledQuestion {Question = _.QuestionText})
                .ToList();
            this.Items = await interviewStateFullViewModelFactory.LoadAsync(id, chapterId);
        }

        public class PrefilledQuestion
        {
            public string Question { get; set; }
            public string Answer { get; set; }
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
        public string QuestionnaireTitle { get; set; }
        public IList PrefilledQuestions { get; set; } 

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