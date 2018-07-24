using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerUserIdentity : IUserIdentity
    {
        Guid SupervisorId { get; }

        string SecurityStamp { get; }
    }
}
