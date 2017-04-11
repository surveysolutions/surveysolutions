using System;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ITroubleshootingService
    {
        string GetMissingDataReason(Guid? interviewId, string interviewKey);
        string GetCensusInterviewsMissingReason(string questionnaireId, Guid? interviewerId, DateTime changedFrom, DateTime changedTo);
    }
}
