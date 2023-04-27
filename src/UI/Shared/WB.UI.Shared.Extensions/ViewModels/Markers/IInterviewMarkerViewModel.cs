using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public interface IInterviewMarkerViewModel : IMarkerViewModel
{
    public InterviewStatus Status { get; }
    //public string InterviewDetails { get; }
    bool CanAssign { get; }
    bool CanApproveReject { get; }
}