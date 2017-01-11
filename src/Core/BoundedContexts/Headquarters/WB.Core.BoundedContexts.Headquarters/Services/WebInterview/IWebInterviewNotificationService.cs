using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Services.WebInterview
{
    public interface IWebInterviewNotificationService
    {
        void RefreshEntities(Guid interviewId, Identity[] questions);
    }
}
