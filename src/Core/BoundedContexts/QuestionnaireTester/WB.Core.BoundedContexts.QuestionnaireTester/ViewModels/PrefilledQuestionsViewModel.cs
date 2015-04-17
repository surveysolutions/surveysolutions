using System.Collections.ObjectModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IPlainInterviewRepository plainStorageInterviewAccessor;
        private string interviewId;

        public PrefilledQuestionsViewModel(ILogger logger,
            IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IPlainInterviewRepository plainStorageInterviewAccessor)
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

        private ObservableCollection<MvxViewModel> prefilledQuestions = new ObservableCollection<MvxViewModel>();
        public ObservableCollection<MvxViewModel> PrefilledQuestions
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

            var interview = this.plainStorageInterviewAccessor.GetInterview(interviewId);
            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireId, interview.QuestionnaireVersion);

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