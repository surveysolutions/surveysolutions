using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentViewFactory
    {
        AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input);
        AssignmentApiView MapAssignment(Assignment assignment);
    }
}