using System.Collections;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private string interviewId;

        public PrefilledQuestionsViewModel(ILogger logger,
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService)
            : base(logger)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public string QuestionnaireTitle { get; set; }

        private IList prefilledQuestions;
        public IList PrefilledQuestions
        {
            get { return prefilledQuestions; }
            set { prefilledQuestions = value; RaisePropertyChanged(); }
        }

        public void Init(string interviewId)
        {
            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = this.interviewViewModelFactory.GetPrefilledQuestions(this.interviewId);
            if (this.NoPrefiiledQuestionsExists())
            {
                this.viewModelNavigationService.NavigateTo<InterviewViewModel>(new { interviewId = this.interviewId });
            }
        }

        private bool NoPrefiiledQuestionsExists()
        {
            return this.PrefilledQuestions.Count == 1;
        }

        public override void NavigateToPreviousViewModel()
        {
            this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
        }
    }
}