using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class InterviewCompletionStatisticsViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;

        public InterviewCompletionStatisticsViewModel(IStatefulInterviewRepository interviewRepository)
        {
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);

            var questionsCount = interview.CountActiveQuestionsInInterview();
            this.AnsweredCount = interview.CountAnsweredQuestionsInInterview();
            this.ErrorsCount = interview.CountInvalidQuestionsInInterview();
            this.UnansweredCount = questionsCount - this.AnsweredCount;
        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }
    }
}
