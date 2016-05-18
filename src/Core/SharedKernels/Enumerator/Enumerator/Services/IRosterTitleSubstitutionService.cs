using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IRosterTitleSubstitutionService
    {
        string Substitute(string title, Identity entityIdentity, string interviewId);
    }
}