using System;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ITroubleshootingService
    {
        string GetMissingDataReason(Guid? interviewId, string interviewKey);
    }
}
