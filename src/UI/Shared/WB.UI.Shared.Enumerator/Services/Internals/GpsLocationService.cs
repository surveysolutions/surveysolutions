using Android.Content;
using Android.Locations;
using Android.OS;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
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

        public async Task<GpsLocation> GetLocation(double desiredAccuracy, AcceptableGpsLocationSource acceptableSource,
            CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>();

            var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);

            if (locationManager == null)
                throw new GpsProviderDisabledException();

            // Modes B (BuiltInGpsOnly) and E (BuiltInOrExternalGps) demand the physical GPS provider.
            // Modes A (AnyNonMock) and N (Any) accept any provider.
            bool requireGpsProvider = acceptableSource.RequiresGpsProvider();
            // Mock locations (external Bluetooth/USB GPS sensors are exposed this way on Android)
            // are only permitted in modes E and N.
            bool allowMock = acceptableSource.AllowsMockProvider();

            if (!IsLocationServicesAvailable(locationManager))
            {
                // When the mode demands the physical GPS sensor, report the missing GPS chip;
                // otherwise the failure is a generic "no suitable provider" rather than absence of GPS.
                if (requireGpsProvider)
                    throw new GpsProviderDisabledException();

                throw new NoSuitableLocationProviderException();
            }

            // Modes B/E require the GPS provider specifically to be enabled.
            if (requireGpsProvider && !locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                throw new GpsProviderDisabledException();

            // Preserve existing contract: canceled requests resolve as timeout/no-fix (null).
            if (cancellationToken.IsCancellationRequested)
                return null;

            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            var listener = new SingleShotLocationListener(tcs, locationManager, desiredAccuracy, requireGpsProvider, allowMock);

            // Enforce a hard 10-minute ceiling regardless of the caller-supplied token.
            using var hardLimitCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, hardLimitCts.Token);
            var effectiveToken = linkedCts.Token;

            // Register first so cancellation callback can always remove listener updates.
            // When the GPS provider is required, register for it exclusively; otherwise register
            // for GPS_PROVIDER explicitly plus every currently-enabled provider so that fixes from
            // an external Bluetooth/USB GPS sensor (which may register under a custom or network
            // provider name via the mock location API) are also received.
            var allProviders = requireGpsProvider
                ? new[] { LocationManager.GpsProvider }.AsEnumerable()
                : locationManager.GetProviders(enabledOnly: true)
                                 .Append(LocationManager.GpsProvider)
                                 .Distinct();
            foreach (var provider in allProviders)
            {
                try { locationManager.RequestLocationUpdates(provider, 0L, 0f, listener); }
                catch { /* provider may have disappeared between enumeration and registration */ }
            }

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
        /// Returns <c>true</c> when location services are available and a fix can be expected.
        /// On API 28+, Android manages location as a single on/off toggle — if location is
        /// enabled, all providers (hardware GPS and any active mock provider for an external
        /// Bluetooth/USB sensor) are accessible.
        /// On API &lt;28, falls back to checking the GPS provider or any available provider.
        /// </summary>
        private static bool IsLocationServicesAvailable(LocationManager locationManager)
        {
            // On API 28+, IsLocationEnabled is the single authoritative flag.
            // IsProviderEnabled("gps") only reflects hardware state and returns false when
            // hardware GPS is off — even when a mock location app (external GPS sensor) is
            // actively injecting fixes into the GPS provider.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                return locationManager.IsLocationEnabled;

            // API < 28: check GPS provider directly, or fall back to any enabled provider.
            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                return true;
            try
            {
                return locationManager.GetProviders(enabledOnly: true).Count > 0;
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
            private readonly bool requireGpsProvider;
            private readonly bool allowMock;

            public SingleShotLocationListener(
                TaskCompletionSource<GpsLocation> tcs,
                LocationManager locationManager,
                double desiredAccuracy,
                bool requireGpsProvider,
                bool allowMock)
            {
                this.tcs = tcs;
                this.locationManager = locationManager;
                this.desiredAccuracy = desiredAccuracy;
                this.requireGpsProvider = requireGpsProvider;
                this.allowMock = allowMock;
            }

            public void OnLocationChanged(AndroidLocation location)
            {
                // Enforce the workspace-configured acceptance criteria: reject fixes that do not
                // come from the required provider, or that are mock when mock is not permitted.
                if (this.requireGpsProvider && location.Provider != LocationManager.GpsProvider)
                    return;
                if (!this.allowMock && location.IsFromMockProvider)
                    return;

                // External GPS devices may emit valid fixes whose elapsedRealtime timestamp
                // does not align with the device monotonic clock. Do not reject by age.

                // Skip fixes that don't meet the configured accuracy threshold — keep
                // waiting for a better satellite fix rather than accepting a coarse one.
                // Non-positive desired accuracy means "accept the first fix" instead of
                // rejecting every accurate reading and timing out.
                // Skip this filter for mock locations (external GPS sensors via Developer
                // Options → "Select mock location app"): they often report a fixed or
                // vendor-specific accuracy value that does not reflect actual signal quality,
                // and blocking on it causes every fix to be rejected until timeout.
                if (!location.IsFromMockProvider)
                {
                    if (desiredAccuracy > 0 && location.HasAccuracy && location.Accuracy > desiredAccuracy)
                        return;
                }

                var timestamp = GetTimestamp(location);
                var gpsLocation = new GpsLocation(
                    location.HasAccuracy ? location.Accuracy : null,
                    location.HasAltitude ? location.Altitude : null,
                    location.Latitude,
                    location.Longitude,
                    timestamp,
                    location.Provider,
                    location.IsFromMockProvider);

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
