using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services;

public interface IGeolocationListener
{
    Task OnGpsLocationChanged(GpsLocation location, INotificationManager notifications);
}


public class GeolocationListenerResult
{
    public GpsLocation Location { get; set; }
    public bool InShapefile { get; set; }

    public GeolocationListenerResult SetCheckResult(bool isShapefile)
    {
        InShapefile = isShapefile;
        return this;
    }
}
