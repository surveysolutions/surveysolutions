using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;

namespace WB.UI.Shared.Extensions.Entities;

public class AssignmentStatusItem
{
    public AssignmentStatusItem(AssignmentStatus? status, string title)
    {
        Status = status;
        Title = title;
    }

    public string Title { get; }
    public AssignmentStatus? Status { get; }
}
