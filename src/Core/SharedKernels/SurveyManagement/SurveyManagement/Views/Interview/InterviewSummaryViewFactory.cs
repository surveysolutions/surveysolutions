using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewSummaryViewFactory : IInterviewSummaryViewFactory
    {
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public InterviewSummaryViewFactory(IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public InterviewSummary Load(string interviewId)
        {
            var interview = this.interviewSummaryReader.GetById(interviewId);
            if (interview == null || interview.IsDeleted)
                return null;

            return interview;
        }
    }
}
