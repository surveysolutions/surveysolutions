using System;
using System.Threading;
using System.Threading.Tasks;
using Geolocator.Plugin.Abstractions;
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

        public async Task<GeoLocation> GetLocation(CancellationToken cancellationToken)
        {
            var position = await this.Geolocator.GetPositionAsync(token: cancellationToken)
                                                .ConfigureAwait(false);
            return new GeoLocation(position.Accuracy, position.Altitude, position.Latitude, position.Longitude, position.Timestamp);
        }
    }
}