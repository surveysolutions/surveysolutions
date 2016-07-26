using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BasePrefilledQuestionsViewModel : SingleInterviewViewModel
    {
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;

        protected BasePrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService)
            : base(principal, viewModelNavigationService, commandService)
        {
            if (interviewViewModelFactory == null) throw new ArgumentNullException(nameof(interviewViewModelFactory));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));
            if (viewModelNavigationService == null) throw new ArgumentNullException(nameof(viewModelNavigationService));

            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.logger = logger;
        }

        public string QuestionnaireTitle { get; set; }

        private ObservableCollection<dynamic> prefilledQuestions;
        public ObservableCollection<dynamic> PrefilledQuestions
        {
            get { return this.prefilledQuestions; }
            set { this.prefilledQuestions = value; this.RaisePropertyChanged(); }
        }

        public void Init(string interviewId) => base.Initialize(interviewId);

        private string currentLanguage;
        public override string CurrentLanguage => this.currentLanguage;

        private IReadOnlyCollection<string> availableLanguages;
        public override IReadOnlyCollection<string> AvailableLanguages => this.availableLanguages;

        public override void Load()
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            var interview = this.interviewRepository.Get(this.interviewId);

            if (interview == null)
            {
                logger.Error("Interview is null. interviewId: " + interviewId);
                viewModelNavigationService.NavigateToDashboard();
                return;
            }

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null) throw new Exception("questionnaire is null. QuestionnaireId: " + interview.QuestionnaireId);

            if (questionnaire.GetPrefilledQuestions().Count == 0)
            {
                this.viewModelNavigationService.NavigateToInterview(interviewId, navigationIdentity: null);
                return;
            }

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = new ObservableCollection<dynamic>();

            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = interview.Language;

            var questions = this.interviewViewModelFactory.GetPrefilledQuestions(this.interviewId);
            questions.ForEach(x => this.PrefilledQuestions.Add(x));

            var startButton = this.interviewViewModelFactory.GetNew<StartInterviewViewModel>();
            startButton.Init(interviewId, null, null);
            this.PrefilledQuestions.Add(startButton);

            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = interview.Language;

            this.IsSuccessfullyLoaded = true;
        }

        public void NavigateToPreviousViewModel() => this.viewModelNavigationService.NavigateToDashboard();
    }
}