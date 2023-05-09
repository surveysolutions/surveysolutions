namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class AssignAssignmentDialogArgs : DoActionDialogArgs
{
    public int AssignmentId { get; }

    public AssignAssignmentDialogArgs(int assignmentId)
    {
        AssignmentId = assignmentId;
    }
}