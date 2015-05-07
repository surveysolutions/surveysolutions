using System.Collections;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private string interviewId;

        public PrefilledQuestionsViewModel(ILogger logger,
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository,
            IStatefulInterviewRepository interviewRepository)
            : base(logger)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public string QuestionnaireTitle { get; set; }

        private IMvxCommand startInterviewCommand;
        public IMvxCommand StartInterviewCommand
        {
            get
            {
                return startInterviewCommand ?? (startInterviewCommand = new MvxCommand(this.StartInterview));
            }
        }

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
        }

        private void StartInterview()
        {
            this.ShowViewModel<InterviewViewModel>(new { interviewId = this.interviewId });
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}