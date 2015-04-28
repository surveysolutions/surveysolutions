using System;
using System.Collections;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;
        private readonly NavigationState navigationState;

        public InterviewViewModel(IPrincipal principal, IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository,
            ChaptersViewModel chaptersViewModel, BreadcrumbsViewModel breadcrumbsViewModel,
            GroupViewModel groupViewModel, NavigationState navigationState)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;

            this.Breadcrumbs = breadcrumbsViewModel;
            this.CurrentGroup = groupViewModel;
            this.Chapters = chaptersViewModel;
        }

        public void Init(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(referenceToQuestion => new
                {
                    InterviewId = interviewId,
                    Question = questionnaire.Questions[referenceToQuestion.Id].Title,
                    AnswerModel = GetAnswerModel(interview, referenceToQuestion)
                })
                .ToList();

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId,
                currentGroupIdentity: new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]));
        }

        private static AbstractInterviewAnswerModel GetAnswerModel(InterviewModel interview, QuestionnaireReferenceModel referenceToQuestion)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestion.Id, new decimal[0]);
            return interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
        }

        public BreadcrumbsViewModel Breadcrumbs { get; set; }
        public GroupViewModel CurrentGroup { get; set; }
        public ChaptersViewModel Chapters { get; set; }
        public string QuestionnaireTitle { get; set; }
        public IEnumerable PrefilledQuestions { get; set; } 

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