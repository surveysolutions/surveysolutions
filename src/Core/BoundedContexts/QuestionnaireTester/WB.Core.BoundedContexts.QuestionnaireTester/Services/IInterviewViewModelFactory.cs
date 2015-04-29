using System.Collections;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IInterviewViewModelFactory
    {
        IList GetEntities(string interviewId, Identity groupIdentity);
        IList GetPrefilledQuestions(string interviewId);
    }
}