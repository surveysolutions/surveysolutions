using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Markers;

public interface IAssignmentMarkerViewModel : IMarkerViewModel
{
    public int AssignmentId { get; }
}