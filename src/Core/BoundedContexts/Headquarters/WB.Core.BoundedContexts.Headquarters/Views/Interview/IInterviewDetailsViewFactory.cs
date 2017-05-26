using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewDetailsViewFactory
    {
        DetailsViewModel GetInterviewDetails(Guid interviewId, InterviewDetailsFilter questionsTypes, Identity currentGroupIdentity = null);

        Guid GetFirstChapterId(Guid id);
    }
}