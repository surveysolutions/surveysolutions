using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Shared.Extensions.Views;

public class GeoTrackingRecord : IPlainStorageEntity<int?>
{
    [PrimaryKey, Unique, AutoIncrement]
    public virtual int? Id { get; set; }

    public Guid InterviewerId { get; set; }
    public int AssignmentId { get; set; }
    
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public bool IsSynchronized { get; set; }
}


public class GeoTrackingPoint : IPlainStorageEntity<int?>
{
    [PrimaryKey, Unique, AutoIncrement]
    public virtual int? Id { get; set; }
    public int GeoTrackingRecordId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset Time { get; set; }
}
