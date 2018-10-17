using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BasePrefilledQuestionsViewModel : SingleInterviewViewModel
    {
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IQuestionnaireStorage questionnaireRepository;
        protected readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;


        protected BasePrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel)
            : base(principal, viewModelNavigationService, commandService, vibrationViewModel)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.logger = logger;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        public string QuestionnaireTitle { get; set; }

        private CompositeCollection<ICompositeEntity> prefilledQuestions;
        public CompositeCollection<ICompositeEntity> PrefilledQuestions
        {
            get { return this.prefilledQuestions; }
            set { this.prefilledQuestions = value; this.RaisePropertyChanged(); }
        }

        private string currentLanguage;
        public override string CurrentLanguage => this.currentLanguage;

        private IReadOnlyCollection<string> availableLanguages;
        public override IReadOnlyCollection<string> AvailableLanguages => this.availableLanguages;

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            var interview = this.interviewRepository.Get(this.InterviewId);

            if (interview == null)
            {
                logger.Error("Interview is null. interviewId: " + InterviewId);
                await viewModelNavigationService.NavigateToDashboardAsync().ConfigureAwait(false);
                return;
            }

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null) throw new Exception("questionnaire is null. QuestionnaireId: " + interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            
            var questions = this.interviewViewModelFactory.GetPrefilledQuestions(this.InterviewId).ToList();

            var visibleSectionItems = this.compositeCollectionInflationService.GetInflatedCompositeCollection(questions);
            
            var startButton = this.interviewViewModelFactory.GetNew<StartInterviewViewModel>();
            startButton.InterviewStarted += (sender, args) => this.Dispose();
            startButton.Init(InterviewId);

            visibleSectionItems.Add(startButton);

            this.PrefilledQuestions = visibleSectionItems;

            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = interview.Language;

            this.IsSuccessfullyLoaded = true;
        }

        public virtual Task NavigateToPreviousViewModelAsync()
        {
            return this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
        }

        public override void Dispose()
        {
            base.Dispose();

            var disposableItems = this.prefilledQuestions.OfType<IDisposable>().ToArray();
            foreach (var disposableItem in disposableItems)
            {
                disposableItem.Dispose();
            }
        }
    }
}
