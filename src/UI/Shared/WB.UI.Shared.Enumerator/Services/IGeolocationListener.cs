using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;

public interface IGeolocationListener
{
    Task OnGpsLocationChanged(GpsLocation location, INotificationManager notifications);
}


public class GeolocationListenerResult
{
    public GpsLocation Location { get; set; }
    public bool OutShapefile { get; set; }

    public GeolocationListenerResult SetCheckResult(bool outShapefile)
    {
        OutShapefile = outShapefile;
        return this;
    }
}
