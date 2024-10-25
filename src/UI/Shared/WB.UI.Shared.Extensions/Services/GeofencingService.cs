using Android.Provider;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Extensions.Services;


public class GeofencingListener : IGeolocationListener
{
    private IVibrationService vibrationService;
    
    private ShapefileFeatureTable shapefile;

    public GeofencingListener(ShapefileFeatureTable shapefile, 
        IVibrationService vibrationService)
    {
        this.shapefile = shapefile;
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

        var queryParameters = new QueryParameters();

        var mapLocation = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);

        var projectedPoint = mapLocation.Project(shapefile.SpatialReference);
        if (projectedPoint is MapPoint mapPoint)
        {
            queryParameters.Geometry = mapPoint;
            queryParameters.SpatialRelationship = SpatialRelationship.Intersects;

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
        if (result.InShapefile)
        {
            notifications.Notify("Out of borders");
        }
    }
}
