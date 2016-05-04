using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IRosterTitleSubstitutionService
    {
        string Substitute(string questionTitle, Identity questionIdentity, string interviewId);
    }
}