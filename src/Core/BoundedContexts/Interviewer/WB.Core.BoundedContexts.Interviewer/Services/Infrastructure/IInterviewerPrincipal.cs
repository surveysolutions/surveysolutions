using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerPrincipal : IPrincipal
    {
        new IInterviewerUserIdentity CurrentUserIdentity { get; }
    }
}
