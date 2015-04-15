using System.Collections.ObjectModel;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory;
        private string interviewId;

        public PrefilledQuestionsViewModel(ILogger logger, IInterviewStateFullViewModelFactory interviewStateFullViewModelFactory)
            : base(logger)
        {
            this.interviewStateFullViewModelFactory = interviewStateFullViewModelFactory;
        }

        private IMvxCommand openInterviewCommand;
        public IMvxCommand OpenInterviewCommand
        {
            get
            {
                return openInterviewCommand ?? (openInterviewCommand = new MvxCommand(this.OpenInterview));
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
            this.PrefilledQuestions = await this.interviewStateFullViewModelFactory.GetPrefilledQuestionsAsync(this.interviewId);
        }

        private void OpenInterview()
        {
            this.ShowViewModel<InterviewGroupViewModel>(new { id = this.interviewId });
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}