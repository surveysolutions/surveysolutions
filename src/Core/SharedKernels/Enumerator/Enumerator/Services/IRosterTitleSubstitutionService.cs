using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IRosterTitleSubstitutionService
    {
        string Substitute(string questionTitle, Identity questionIdentity, string interviewId);
    }
}