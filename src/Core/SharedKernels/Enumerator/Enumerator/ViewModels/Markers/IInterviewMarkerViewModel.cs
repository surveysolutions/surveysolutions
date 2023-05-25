using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Markers;

public interface IInterviewMarkerViewModel : IMarkerViewModel
{
    public InterviewStatus InterviewStatus { get; }
}