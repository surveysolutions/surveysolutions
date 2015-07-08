using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SideBarPrefillQuestion
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class InterviewViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly NavigationState navigationState;
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewViewModel(IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel, 
            BreadCrumbsViewModel breadCrumbsViewModel,
            ActiveGroupViewModel groupViewModel, 
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;
            this.answerNotifier = answerNotifier;
            this.answerToStringService = answerToStringService;
            this.viewModelNavigationService = viewModelNavigationService;

            this.BreadCrumbs = breadCrumbsViewModel;
            this.CurrentGroup = groupViewModel;
            this.Sections = sectionsViewModel;
        }

        public async Task Init(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(referenceToQuestion => new SideBarPrefillQuestion
                {
                    Question = questionnaire.Questions[referenceToQuestion.Id].Title,
                    Answer = GetAnswer(interview, questionnaire, referenceToQuestion)
                })
                .ToList();

            this.BreadCrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(questionnaire.Id.FormatGuid(), interviewId, this.navigationState);
            this.CurrentGroup.Init(this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.OnGroupChanged += NavigationStateOnOnGroupChanged;
            await this.navigationState.NavigateTo(groupIdentity: new Identity(questionnaire.GroupsWithFirstLevelChildrenAsReferences.Keys.First(), new decimal[0]));

            this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
        }

        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            this.UpdateInterviewStatus(navigationState.CurrentGroup);
        }

        private void NavigationStateOnOnGroupChanged(GroupChangedEventArgs newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(navigationState.InterviewId);
            IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(newGroupIdentity.TargetGroup);
            this.answerNotifier.Init(questionsToListen.ToArray());

            this.UpdateInterviewStatus(newGroupIdentity.TargetGroup);
        }

        private void UpdateInterviewStatus(Identity groupIdentity)
        {
            var interview = this.interviewRepository.Get(navigationState.InterviewId);

            var state = new GroupState(groupIdentity);
            state.UpdateSelfFromModel(interview);

            this.Status = state.Status;
        }

        private string GetAnswer(IStatefulInterview interview, QuestionnaireModel questionnaire, QuestionnaireReferenceModel referenceToQuestion)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestion.Id, new decimal[0]);
            var interviewAnswer = interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
            var questionModel = questionnaire.Questions[referenceToQuestion.Id];
            return answerToStringService.AnswerToUIString(questionModel, interviewAnswer);
        }

        private GroupStatus status;
        public GroupStatus Status
        {
            get { return this.status; }
            private set
            {
                if (status != value)
                {
                    this.status = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public BreadCrumbsViewModel BreadCrumbs { get; set; }
        public ActiveGroupViewModel CurrentGroup { get; set; }
        public SideBarSectionsViewModel Sections { get; set; }
        public string QuestionnaireTitle { get; set; }
        public IEnumerable PrefilledQuestions { get; set; } 

        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get
            {
                return navigateToDashboardCommand ?? (navigateToDashboardCommand = new MvxCommand(() => this.viewModelNavigationService.NavigateTo<DashboardViewModel>()));
            }
        }

        private IMvxCommand navigateToHelpCommand;
        public IMvxCommand NavigateToHelpCommand
        {
            get
            {
                return navigateToHelpCommand ?? (navigateToHelpCommand = new MvxCommand(() => this.viewModelNavigationService.NavigateTo<HelpViewModel>()));
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

        private void SignOut()
        {
            this.principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        private void ChangeLanguage()
        {
            throw new NotImplementedException();
        }

        public override void NavigateToPreviousViewModel()
        {
            this.navigationState.NavigateBack(()=>this.viewModelNavigationService.NavigateTo<DashboardViewModel>()).Wait();
        }
    }
}