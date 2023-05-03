using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Markers;

public interface IMarkerViewModel
{
    string Id { get; }
    MarkerType Type { get; }
    public double Latitude { get; }
    public double Longitude { get; }
}
