namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentViewFactory
    {
        AssignmentsWithoutIdentifingData Load(AssignmentsInputModel input);
    }
}