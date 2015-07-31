using System;
using System.Collections;
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
            : base()
        {
            if (interviewViewModelFactory == null) throw new ArgumentNullException("interviewViewModelFactory");
            if (plainQuestionnaireRepository == null) throw new ArgumentNullException("plainQuestionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (viewModelNavigationService == null) throw new ArgumentNullException("viewModelNavigationService");

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
            if (interviewId == null) throw new ArgumentNullException("interviewId");

            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(this.interviewId);
            if (interview == null) throw new Exception("Interview is null.");

            var questionnaire = this.plainQuestionnaireRepository.GetById(interview.QuestionnaireId);
            if (questionnaire == null) throw new Exception("questionnaire is null");
            
            this.QuestionnaireTitle = questionnaire.Title;
            if (questionnaire.PrefilledQuestionsIds.Count == 0)
            {
                this.viewModelNavigationService.NavigateTo<InterviewViewModel>(new { interviewId = this.interviewId });
            }

            this.PrefilledQuestions = this.interviewViewModelFactory.GetPrefilledQuestions(this.interviewId);
        }
        
        public override void NavigateToPreviousViewModel()
        {
            this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
        }
    }
}