using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewDetailsViewFactory
    {
        DetailsViewModel GetInterviewDetails(Guid interviewId, InterviewDetailsFilter filter, Identity currentGroupIdentity = null);
    }
}