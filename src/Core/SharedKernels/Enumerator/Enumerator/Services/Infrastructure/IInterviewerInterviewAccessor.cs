using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IInterviewerInterviewAccessor
    {
        void RemoveInterview(Guid interviewId);
        InterviewPackageApiView GetInteviewEventsPackageOrNull(Guid interviewId);
        Task CreateInterviewAsync(InterviewApiView info, InterviewerInterviewApiView details);
    }
}
