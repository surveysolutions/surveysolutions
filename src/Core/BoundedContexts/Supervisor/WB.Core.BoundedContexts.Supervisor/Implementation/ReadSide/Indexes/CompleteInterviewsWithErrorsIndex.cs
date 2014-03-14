using System;
using System.Linq;
using Raven.Client.Indexes;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide.Indexes
{
    public class CompleteInterviewsWithErrorsIndex :
        AbstractIndexCreationTask<InterviewSummary, CompleteInterviewsWithErrorsIndex.CompleteInterviewsWithErrors>
    {
        public class CompleteInterviewsWithErrors
        {
            public Guid InterviewId { get; set; }
        }

        public CompleteInterviewsWithErrorsIndex()
        {
            this.Map = interviews => from interview in interviews
                where
                    interview.HasErrors && interview.IsDeleted == false &&
                        (interview.Status == InterviewStatus.Completed || 
                         interview.Status == InterviewStatus.RejectedBySupervisor ||
                         interview.Status == InterviewStatus.ApprovedBySupervisor ||
                         interview.Status == InterviewStatus.ApprovedByHeadquarters ||
                         interview.Status == InterviewStatus.RejectedByHeadquarters)

                select new CompleteInterviewsWithErrors { InterviewId = interview.InterviewId };
        }
    }
}
