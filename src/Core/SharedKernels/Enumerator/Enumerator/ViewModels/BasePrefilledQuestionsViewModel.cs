using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly ILogger logger;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;


        protected BasePrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            IEnumeratorSettings enumeratorSettings,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel)
            : base(principal, viewModelNavigationService, commandService, enumeratorSettings, vibrationViewModel)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
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
            get => this.prefilledQuestions; 
            set { this.prefilledQuestions = value; this.RaisePropertyChanged(); }
        }

        private string currentLanguage;
        public override string CurrentLanguage => this.currentLanguage;
        
        private string defaultLanguageName;
        public override string DefaultLanguageName => this.defaultLanguageName;

        private IReadOnlyCollection<string> availableLanguages;
        private StartInterviewViewModel startButton;
        public override IReadOnlyCollection<string> AvailableLanguages => this.availableLanguages;

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            var interview = this.interviewRepository.Get(this.InterviewId);

            if (interview == null)
            {
                logger.Error("Interview is null. interviewId: " + InterviewId);
                await ViewModelNavigationService.NavigateToDashboardAsync().ConfigureAwait(false);
                return;
            }

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null) throw new Exception("questionnaire is null. QuestionnaireId: " + interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;

            var navigationState = this.interviewViewModelFactory.GetNew<NavigationState>();
            navigationState.Init(InterviewId, questionnaire.QuestionnaireId.FormatGuid());
            var prefilledEntities = this.interviewViewModelFactory.GetPrefilledEntities(this.InterviewId, navigationState).ToList();

            var visibleSectionItems = this.compositeCollectionInflationService.GetInflatedCompositeCollection(prefilledEntities);
            
            this.startButton = this.interviewViewModelFactory.GetNew<StartInterviewViewModel>();
            startButton.InterviewStarted += (sender, args) => this.Dispose();
            startButton.Init(InterviewId);

            visibleSectionItems.Add(startButton);

            this.PrefilledQuestions = visibleSectionItems;

            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = interview.Language;
            this.defaultLanguageName = questionnaire.DefaultLanguageName;

            this.IsAudioRecordingEnabled = interview.GetIsAudioRecordingEnabled();

            this.IsSuccessfullyLoaded = true;
        }

        public bool? IsAudioRecordingEnabled { get; set; }

        public virtual Task NavigateToPreviousViewModelAsync()
        {
            return this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
        }

        public Task StartInterviewAsync() => this.startButton.StartInterviewCommand.ExecuteAsync();

        public override void Dispose()
        {
            base.Dispose();

            PrefilledQuestions.ForEach(viewModel=> viewModel.DisposeIfDisposable());
        }
    }
}
