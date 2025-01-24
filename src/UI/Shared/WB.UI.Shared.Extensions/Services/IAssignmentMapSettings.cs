namespace WB.UI.Shared.Extensions.Services;

public interface IAssignmentMapSettings
{
    bool AllowGeoTracking { get; }
    bool AllowGeofencing { get; }
    bool AllowCreateInterview { get; }
}

public class InterviewerAssignmentMapSettings : IAssignmentMapSettings
{
    public bool AllowGeoTracking => true;
    public bool AllowGeofencing => true;
    public bool AllowCreateInterview => true;
}

public class SupervisorAssignmentMapSettings : IAssignmentMapSettings
{
    public bool AllowGeoTracking => false;
    public bool AllowGeofencing => false;
    public bool AllowCreateInterview => false;
}
