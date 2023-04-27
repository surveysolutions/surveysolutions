namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public interface IMarkerViewModel
{
    string Id { get; }
    MarkerType Type { get; }
    public double Latitude { get; }
    public double Longitude { get; }
}