using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
             IPlainRepository<InterviewModel> interviewRepository)
        {
            this.principal = principal;
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public async void Init(string id, string chapterId)
        {
            var interview = this.interviewRepository.Get(id);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId.FormatGuid());

            var groupId = string.IsNullOrEmpty(chapterId) 
                ? questionnaire.GroupsWithoutNestedChildren.Keys.First()
                : Guid.Parse(chapterId);

            var group = questionnaire.GroupsWithoutNestedChildren[groupId];

            this.CurrentGroupName = group.Title;
            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(questionId => new PrefilledQuestion
                              {
                                  Question = questionnaire.Questions[questionId].Title,
                                  Answer = string.Empty
                              })
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