namespace WB.UI.Shared.Extensions.Services;

public interface IAssignmentMapSettings
{
    bool AllowGeoTracking { get; }
    bool AllowGeofencing { get; }
}

public class InterviewerAssignmentMapSettings : IAssignmentMapSettings
{
    public bool AllowGeoTracking => true;
    public bool AllowGeofencing => true;
}

public class SupervisorAssignmentMapSettings : IAssignmentMapSettings
{
    public bool AllowGeoTracking => true;
    public bool AllowGeofencing => true;
}
