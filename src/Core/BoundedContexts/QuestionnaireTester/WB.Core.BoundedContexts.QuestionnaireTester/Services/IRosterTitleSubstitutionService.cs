using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IRosterTitleSubstitutionService
    {
        string Substitute(string questionTitle, Identity questionIdentity, string interviewId);
    }
}