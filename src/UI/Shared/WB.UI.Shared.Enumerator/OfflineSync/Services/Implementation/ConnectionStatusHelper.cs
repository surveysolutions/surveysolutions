using System;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public static class ConnectionStatusHelper
    {
        public static async Task<NearbyStatus> ToConnectionStatus(this Task<Statuses> statuses)
        {
            var status = await statuses;
            return status.ToConnectionStatus();
        }

        public static NearbyStatus ToConnectionStatus(this Statuses statuses)
        {
            var status = Enum.IsDefined(typeof(ConnectionStatusCode), statuses.StatusCode)
                ? (ConnectionStatusCode) statuses.StatusCode
                : ConnectionStatusCode.Unknown;

            return new NearbyStatus
            {
                IsSuccess = statuses.IsSuccess,
                IsCanceled = statuses.IsCanceled,
                StatusMessage = statuses.StatusMessage,
                IsInterrupted = statuses.IsInterrupted,
                StatusCode = statuses.StatusCode,
                Status = status
            };
        }
    }
}
