using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmetnsDeletionService
    {
        void Delete(QuestionnaireIdentity questionnaireIdentity);
    }
}