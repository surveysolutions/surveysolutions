using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.GeoTracking;

public class GeoTrackingRecord
{
    public virtual long Id { get; set; }
    public virtual Guid InterviewerId { get; set; }
    public virtual int AssignmentId { get; set; }
    public virtual DateTime Start { get; set; }
    public virtual DateTime? End { get; set; }
    public virtual IList<GeoTrackingPoint> Points { get; set; }
}

public class GeoTrackingPoint 
{
    public virtual double Latitude { get; set; }
    public virtual double Longitude { get; set; }
    public virtual DateTime Time { get; set; }
}

