using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewSummaryViewFactory : IInterviewSummaryViewFactory
    {
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public InterviewSummaryViewFactory(IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public InterviewSummary Load(Guid interviewId)
        {
            var interview = this.interviewSummaryReader.GetById(interviewId);

            return interview;
        }
    }
}
