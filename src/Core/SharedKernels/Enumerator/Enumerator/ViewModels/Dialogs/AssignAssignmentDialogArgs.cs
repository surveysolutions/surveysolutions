namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class AssignAssignmentDialogArgs : IDoActionDialogArgs
{
    public int AssignmentId { get; }

    public AssignAssignmentDialogArgs(int assignmentId)
    {
        AssignmentId = assignmentId;
    }
}