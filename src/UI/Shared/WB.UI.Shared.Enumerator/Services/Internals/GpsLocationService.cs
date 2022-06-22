using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class GpsLocationService : IGpsLocationService
    {
        private readonly IPermissionsService permissions;

        public GpsLocationService(IPermissionsService permissions)
        {
            this.permissions = permissions;
        }

        public async Task<GpsLocation> GetLocation(int timeoutSec, double desiredAccuracy)
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>();
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSec));
            var geolocationAccuracy = GetGeolocationAccuracy(desiredAccuracy);
            GeolocationRequest geolocationRequest = new GeolocationRequest(geolocationAccuracy);
            var position = await Geolocation.GetLocationAsync(
                geolocationRequest, cancellationToken.Token).ConfigureAwait(false);

            return new GpsLocation(position.Accuracy, position.Altitude, position.Latitude, position.Longitude, position.Timestamp);
        }

        private static GeolocationAccuracy GetGeolocationAccuracy(double desiredAccuracy)
        {
            if (desiredAccuracy < 100)
                return GeolocationAccuracy.High;
            if (desiredAccuracy < 500)
                return GeolocationAccuracy.Medium;
            return GeolocationAccuracy.Low;
        }
    }
}
