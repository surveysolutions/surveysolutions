using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class GpsLocationService : IGpsLocationService
    {
        private readonly IGeolocator Geolocator;
      
        public GpsLocationService(IGeolocator geolocator)
        {
            this.Geolocator = geolocator;
        }

        public async Task<GpsLocation> GetLocation(int timeoutSec, double desiredAccuracy)
        {
            this.Geolocator.DesiredAccuracy = desiredAccuracy;
            Position position = await this.Geolocator.GetPositionAsync(timeoutMilliseconds: timeoutSec*1000)
                                                     .ConfigureAwait(false);
            IPermissions permissions = CrossPermissions.Current;
            var gpsPermissionStatus = await permissions.CheckPermissionStatusAsync(Permission.Location)
                                                                    .ConfigureAwait(false);
            if (gpsPermissionStatus != PermissionStatus.Granted && position == null)
            {
                throw new NoGpsPermissionException();
            }

            return new GpsLocation(position.Accuracy, position.Altitude, position.Latitude, position.Longitude, position.Timestamp);
        }
    }
}