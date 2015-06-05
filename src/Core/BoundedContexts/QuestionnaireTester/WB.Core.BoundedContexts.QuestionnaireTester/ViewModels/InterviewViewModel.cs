using System;
using System.Collections;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class InterviewViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly NavigationState navigationState;

        public InterviewViewModel(IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            SectionsViewModel sectionsViewModel, 
            BreadcrumbsViewModel breadcrumbsViewModel,
            ActiveGroupViewModel groupViewModel, 
            NavigationState navigationState)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;

            this.Breadcrumbs = breadcrumbsViewModel;
            this.CurrentGroup = groupViewModel;
            this.Sections = sectionsViewModel;
        }

        public void Init(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(referenceToQuestion => new
                {
                    InterviewId = interviewId,
                    Question = questionnaire.Questions[referenceToQuestion.Id].Title,
                    AnswerModel = GetAnswerModel(interview, referenceToQuestion)
                })
                .ToList();

            this.Breadcrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(questionnaire.Id.FormatGuid(), this.navigationState);
            this.CurrentGroup.Init(this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.NavigateTo(groupIdentity: new Identity(questionnaire.GroupsWithoutNestedChildren.Keys.First(), new decimal[0]));
        }

        private static BaseInterviewAnswer GetAnswerModel(IStatefulInterview interview, QuestionnaireReferenceModel referenceToQuestion)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestion.Id, new decimal[0]);
            return interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
        }

        public BreadcrumbsViewModel Breadcrumbs { get; set; }
        public ActiveGroupViewModel CurrentGroup { get; set; }
        public SectionsViewModel Sections { get; set; }
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
            this.navigationState.NavigateBack(()=>this.ShowViewModel<DashboardViewModel>());
        }
    }
}