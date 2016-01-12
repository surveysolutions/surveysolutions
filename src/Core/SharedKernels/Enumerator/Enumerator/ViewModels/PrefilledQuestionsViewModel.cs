using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILogger logger;
        protected string interviewId;

        public PrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger)
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

        public async void Init(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(this.interviewId);
            if (interview == null)
            {
                logger.Error("Interview is null. interviewId: " + interviewId);
                await viewModelNavigationService.NavigateToDashboardAsync();
                return;
            }

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            if (questionnaire == null) throw new Exception("questionnaire is null. QuestionnaireId: " + interview.QuestionnaireId);

            if (questionnaire.GetPrefilledQuestions().Count == 0)
            {
                await this.viewModelNavigationService.NavigateToInterviewAsync(interviewId);
                return;
            }

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = new ObservableCollection<dynamic>();

            this.interviewViewModelFactory.GetPrefilledQuestions(this.interviewId)
                .ForEach(x => this.PrefilledQuestions.Add(x));

            var startButton = this.interviewViewModelFactory.GetNew<StartInterviewViewModel>();
            startButton.Init(interviewId, null, null);
            this.PrefilledQuestions.Add(startButton);
        }

        public async Task NavigateToPreviousViewModelAsync()
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync();
        }
    }
}