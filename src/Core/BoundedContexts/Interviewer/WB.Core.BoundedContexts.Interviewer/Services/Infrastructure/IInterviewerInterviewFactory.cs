using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerInterviewFactory
    {
        Task RemoveInterviewAsync(Guid interviewId);
        string GetPackageByCompletedInterview(Guid interviewId);
        Task CreateInterviewAsync(InterviewApiView info, InterviewDetailsApiView details);
    }
}