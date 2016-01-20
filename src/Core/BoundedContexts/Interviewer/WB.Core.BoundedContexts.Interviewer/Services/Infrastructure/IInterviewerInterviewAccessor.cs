using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerInterviewAccessor
    {
        Task RemoveInterviewAsync(Guid interviewId);
        Task<string> GetPackageByCompletedInterviewAsync(Guid interviewId);
        Task CreateInterviewAsync(InterviewApiView info, InterviewDetailsApiView details);
    }
}