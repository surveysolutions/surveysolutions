using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi;

public class GeoTrackingPackageApiView
{
    public GeoTrackingRecordApiView[] Records { get; set; }
}


public class GeoTrackingRecordApiView
{
    public Guid InterviewerId { get; set; }
    public int AssignmentId { get; set; }
    
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    
    public GeoTrackingPointApiView[] Points { get; set; }
}


public class GeoTrackingPointApiView
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset Time { get; set; }
}
