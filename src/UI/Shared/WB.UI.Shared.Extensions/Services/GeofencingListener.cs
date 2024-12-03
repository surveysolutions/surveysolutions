using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Extensions.Services;

public interface IGeofencingListener : IGeolocationListener
{
    void Start(ShapefileFeatureTable shapefile);

    GeolocationListenerResult LastResult { get; }
}

public class GeofencingListener : IGeofencingListener
{
    private IVibrationService vibrationService;
    
    private ShapefileFeatureTable shapefile;

    public GeofencingListener(
        IVibrationService vibrationService)
    {
        this.vibrationService = vibrationService;
    }

    public async Task<GeolocationListenerResult> CheckIfInsideOfShapefile(GpsLocation location)
    {
        var geofencingResult = new GeolocationListenerResult()
        {
            Location = location,
        };
        
        if (shapefile?.SpatialReference == null)
            return geofencingResult.SetCheckResult(false);
        
        var mapLocation = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);

        var projectedPoint = mapLocation.Project(shapefile.SpatialReference);
        if (projectedPoint is MapPoint mapPoint)
        {
            var queryParameters = new QueryParameters
            {
                Geometry = mapPoint,
                SpatialRelationship = SpatialRelationship.Intersects
            };

            var queryResult = await shapefile.QueryFeaturesAsync(queryParameters);
            if (!queryResult.Any())
            {
                vibrationService.Enable();
                vibrationService.Vibrate();
                return geofencingResult.SetCheckResult(true);
            }
            else
            {
                vibrationService.Disable();
            }
        }

        return geofencingResult.SetCheckResult(false);
    }
    
    public async Task OnGpsLocationChanged(GpsLocation location, INotificationManager notifications)
    {
        var result = await CheckIfInsideOfShapefile(location);
        if (result.OutShapefile)
        {
            notifications.Notify("Out of borders");
        }

        LastResult = result;
    }

    public void Start(ShapefileFeatureTable shapefile)
    {
        this.shapefile = shapefile;
    }

    public GeolocationListenerResult LastResult { get; set; }
}
