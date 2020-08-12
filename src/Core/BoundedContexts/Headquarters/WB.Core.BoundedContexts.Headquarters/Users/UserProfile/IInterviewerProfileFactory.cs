using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile
{
    public interface IInterviewerProfileFactory
    {
        Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid interviewerId);

        ReportView GetInterviewersReport(Guid[] interviewersIdsToExport);

        InterviewerPoints GetInterviewerCheckInPoints(Guid interviewerId);

        Task<InterviewerTrafficUsage> GetInterviewerTrafficUsageAsync(Guid interviewerId);
    }
}
