using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class InterviewCompletionStatisticsViewModel : MvxViewModel, IInterviewEntityViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public InterviewCompletionStatisticsViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        private string interviewId;

        public Identity Identity { get { return null; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(this.interviewId);

            var questionsCount = interview.CountActiveQuestionsInInterview();
            this.AnsweredCount = interview.CountAnsweredQuestionsInInterview();
            this.ErrorsCount = interview.CountInvalidQuestionsInInterview();
            this.UnansweredCount = questionsCount - this.AnsweredCount;
        }

        private int answeredCount;
        public int AnsweredCount
        {
            get { return this.answeredCount; }
            set { this.answeredCount = value; this.RaisePropertyChanged(); }
        }

        private int unansweredCount;
        public int UnansweredCount
        {
            get { return this.unansweredCount; }
            set { this.unansweredCount = value; this.RaisePropertyChanged(); }
        }

        private int errorsCount;
        public int ErrorsCount
        {
            get { return this.errorsCount; }
            set { this.errorsCount = value; this.RaisePropertyChanged(); }
        }
    }
}
