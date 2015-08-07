using Cirrious.MvvmCross.ViewModels;

using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels.InterviewDetails
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
            AnsweredCount = interview.CountAnsweredQuestionsInInterview();
            ErrorsCount = interview.CountInvalidQuestionsInInterview();
            UnansweredCount = questionsCount - AnsweredCount;
        }

        private int answeredCount;
        public int AnsweredCount
        {
            get { return this.answeredCount; }
            set { this.answeredCount = value; RaisePropertyChanged(); }
        }

        private int unansweredCount;
        public int UnansweredCount
        {
            get { return this.unansweredCount; }
            set { this.unansweredCount = value; RaisePropertyChanged(); }
        }

        private int errorsCount;
        public int ErrorsCount
        {
            get { return this.errorsCount; }
            set { this.errorsCount = value; RaisePropertyChanged(); }
        }
    }
}
