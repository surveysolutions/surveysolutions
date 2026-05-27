using Android.Content;
using Android.Locations;
using Android.OS;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using Xamarin.Essentials;
using AndroidLocation = Android.Locations.Location;
using Application = Android.App.Application;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class GpsLocationService : IGpsLocationService
    {
        private readonly IPermissionsService permissions;

        public GpsLocationService(IPermissionsService permissions)
        {
            this.permissions = permissions;
        }

        public async Task<GpsLocation> GetLocation(double desiredAccuracy, CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>();

            var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);

            if (locationManager == null)
                throw new GpsProviderDisabledException();

            // Accept hardware GPS *or* an active mock provider for the GPS provider —
            // the latter is how external Bluetooth/USB GPS sensors are exposed on Android
            // when "Allow mock locations" is enabled in Developer Settings.
            if (!IsGpsProviderAvailable(locationManager))
                throw new GpsProviderDisabledException();

            // Preserve existing contract: canceled requests resolve as timeout/no-fix (null).
            if (cancellationToken.IsCancellationRequested)
                return null;

            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Capture the request time *before* registering the listener so that any fix
            // whose timestamp predates this moment is recognised as a stale cached value
            // and discarded. This guards against mock/external-sensor providers that may
            // replay a previously injected position as the very first callback.
            var requestTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var listener = new SingleShotLocationListener(tcs, locationManager, desiredAccuracy, requestTimeMs);

            // Enforce a hard 10-minute ceiling regardless of the caller-supplied token.
            using var hardLimitCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, hardLimitCts.Token);
            var effectiveToken = linkedCts.Token;

            // Register first so cancellation callback can always remove listener updates.
            locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0L, 0f, listener);

            // Register cancellation: remove the listener and complete with null when the
            // CancellationToken fires (e.g. the user-configured GpsReceiveTimeoutSec elapses or
            // the hard 10-minute limit is reached).
            using var registration = effectiveToken.Register(() =>
            {
                try { locationManager.RemoveUpdates(listener); } catch { /* ignore – listener may already be unregistered */ }
                tcs.TrySetResult(null);
            });

            // If cancellation happened between request and registration, complete deterministically.
            if (effectiveToken.IsCancellationRequested)
            {
                try { locationManager.RemoveUpdates(listener); } catch { /* ignore */ }
                tcs.TrySetResult(null);
            }

            return await tcs.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Returns <c>true</c> when a GPS fix can be expected — either from the built-in
        /// hardware GPS chip or from an external sensor (Bluetooth / USB) that injects fixes
        /// into the GPS provider via an Android mock location app (Developer Settings →
        /// "Allow mock locations" / "Select mock location app").
        /// <para>
        /// <see cref="LocationManager.GetProviders(bool)"/> with <c>enabledOnly = true</c>
        /// includes the GPS provider whenever it is active, whether through real hardware
        /// or through a registered mock-location provider — unlike
        /// <see cref="LocationManager.IsProviderEnabled"/> which only reflects the hardware
        /// toggle in the device Location settings.
        /// </para>
        /// </summary>
        private static bool IsGpsProviderAvailable(LocationManager locationManager)
        {
            // Primary check: hardware GPS enabled.
            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                return true;

            // Secondary check: a mock location app is active for the GPS provider
            // (external sensor scenario).
            try
            {
                return locationManager.GetProviders(enabledOnly: true)
                                      .Contains(LocationManager.GpsProvider);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// One-shot <see cref="ILocationListener"/> that resolves a
        /// <see cref="TaskCompletionSource{GpsLocation}"/> on the first GPS fix that meets
        /// the requested accuracy, then unregisters itself.
        /// </summary>
        private sealed class SingleShotLocationListener : Java.Lang.Object, ILocationListener
        {
            private readonly TaskCompletionSource<GpsLocation> tcs;
            private readonly LocationManager locationManager;
            private readonly double desiredAccuracy;
            private readonly long requestTimeMs;

            public SingleShotLocationListener(
                TaskCompletionSource<GpsLocation> tcs,
                LocationManager locationManager,
                double desiredAccuracy,
                long requestTimeMs)
            {
                this.tcs = tcs;
                this.locationManager = locationManager;
                this.desiredAccuracy = desiredAccuracy;
                this.requestTimeMs = requestTimeMs;
            }

            public void OnLocationChanged(AndroidLocation location)
            {
                // Reject fixes that predate the request — these are cached/stale positions
                // that some providers (especially mock/external-sensor providers) may deliver
                // as the very first callback before any fresh fix is available.
                if (location.Time < requestTimeMs)
                    return;

                // Skip fixes that don't meet the configured accuracy threshold — keep
                // waiting for a better satellite fix rather than accepting a coarse one.
                // Non-positive desired accuracy means "accept the first fix" instead of
                // rejecting every accurate reading and timing out.
                if (desiredAccuracy > 0 && location.HasAccuracy && location.Accuracy > desiredAccuracy)
                    return;

                var timestamp = GetTimestamp(location);
                var gpsLocation = new GpsLocation(
                    location.HasAccuracy ? location.Accuracy : null,
                    location.HasAltitude ? location.Altitude : null,
                    location.Latitude,
                    location.Longitude,
                    timestamp);

                // Set result before removing updates so the task always completes even if
                // RemoveUpdates throws (e.g. when called from a non-looper thread).
                tcs.TrySetResult(gpsLocation);

                // Unregister so we act as a one-shot listener.
                try { locationManager.RemoveUpdates(this); } catch { /* ignore – result already set */ }
            }

            public void OnProviderDisabled(string provider) { }
            public void OnProviderEnabled(string provider) { }
            public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

            private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            private static DateTimeOffset GetTimestamp(AndroidLocation location)
            {
                try { return new DateTimeOffset(Epoch.AddMilliseconds(location.Time)); }
                catch { return DateTimeOffset.UtcNow; }
            }
        }
    }
}
