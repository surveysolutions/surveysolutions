using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class GpsLocationService : IGpsLocationService
    {
        private readonly IGeolocator Geolocator;
        private readonly IPermissions permissions;

        public GpsLocationService(IGeolocator geolocator, IPermissions permissions)
        {
            this.Geolocator = geolocator;
            this.permissions = permissions;
        }

        public async Task<GpsLocation> GetLocation(int timeoutSec, double desiredAccuracy)
        {
            await this.permissions.AssureHasPermission(Permission.Location);
            this.Geolocator.DesiredAccuracy = desiredAccuracy;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSec));
            Position position = await this.Geolocator.GetPositionAsync(token: cancellationToken.Token)
                                                     .ConfigureAwait(false);

            return new GpsLocation(position.Accuracy, position.Altitude, position.Latitude, position.Longitude, position.Timestamp);
        }
    }
}