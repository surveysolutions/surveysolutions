using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsToDeleteFactory
    {
        Task RemoveAllAssignmentsDataAsync(QuestionnaireIdentity questionnaireIdentity);
    }
}
