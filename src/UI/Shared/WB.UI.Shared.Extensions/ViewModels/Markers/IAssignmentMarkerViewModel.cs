namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public interface IAssignmentMarkerViewModel : IMarkerViewModel
{
    public bool CanCreateInterview { get; }
    public bool CanAssign { get; }
}