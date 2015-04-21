using System.Collections;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;
        private readonly IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository;
        private readonly IPlainRepository<InterviewModel> plainStorageInterviewAccessor;
        private string interviewId;

        public PrefilledQuestionsViewModel(ILogger logger,
            IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory,
            IPlainRepository<QuestionnaireModel> plainQuestionnaireRepository,
            IPlainRepository<InterviewModel> plainStorageInterviewAccessor)
            : base(logger)
        {
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.plainStorageInterviewAccessor = plainStorageInterviewAccessor;
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

        private IEnumerable prefilledQuestions;
        public IEnumerable PrefilledQuestions
        {
            get { return prefilledQuestions; }
            set
            {
                prefilledQuestions = value;
                RaisePropertyChanged(() => PrefilledQuestions);
            }
        }

        public async void Init(string interviewId)
        {
            this.interviewId = interviewId;

            var interview = this.plainStorageInterviewAccessor.Get(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.Get(interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = await this.interviewStateFullViewModelFactory.GetPrefilledQuestionsAsync(this.interviewId);
        }

        private void StartInterview()
        {
            this.ShowViewModel<InterviewGroupViewModel>(new { id = this.interviewId });
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}