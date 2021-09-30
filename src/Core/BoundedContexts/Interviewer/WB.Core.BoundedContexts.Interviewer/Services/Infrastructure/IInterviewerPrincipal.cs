using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerPrincipal : IPrincipal
    {
        //new IInterviewerUserIdentity CurrentUserIdentity { get; }
        
        bool DoesIdentityExist();

        string? GetExistingIdentityNameOrNull();

        bool SaveInterviewer(InterviewerIdentity interviewer);

        InterviewerIdentity GetInterviewerByName(string name);
    }
}
