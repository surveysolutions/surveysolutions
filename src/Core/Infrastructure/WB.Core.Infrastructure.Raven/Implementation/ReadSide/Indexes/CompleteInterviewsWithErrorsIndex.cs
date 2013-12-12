using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.Indexes
{
    public class CompleteInterviewsWithErrorsIndex : AbstractIndexCreationTask<InterviewSummary, CompleteInterviewsWithErrorsIndex.CompleteInterviewsWithErrors>
    {
        public class CompleteInterviewsWithErrors
        {
            public Guid InterviewId { get; set; }
        }

        public CompleteInterviewsWithErrorsIndex()
        {
            Map = interviews => from interview in interviews
                                where interview.HasErrors && interview.IsDeleted == false && (interview.Status == InterviewStatus.Completed || interview.Status == InterviewStatus.RejectedBySupervisor || interview.Status == InterviewStatus.ApprovedBySupervisor)
                                select new CompleteInterviewsWithErrors { InterviewId = interview.InterviewId };
        }
    }
}
