using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Markers;

public interface IInterviewMarkerViewModel : IMarkerViewModel
{
    public InterviewStatus InterviewStatus { get; }
}