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
            if (!acceptableSource.IsKnownValue())
                throw new NoSuitableLocationProviderException();

            if (!IsLocationServicesAvailable(locationManager))
            {
                // When the mode demands the physical GPS sensor, report the missing GPS chip;
                // otherwise the failure is a generic "no suitable provider" rather than absence of GPS.
                if (requireGpsProvider)
                    throw new GpsProviderDisabledException();

                throw new NoSuitableLocationProviderException();
            }

            // Only mode B requires the hardware GPS provider to be enabled.
            if (acceptableSource.RequiresEnabledGpsProvider() &&
                !locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                throw new GpsProviderDisabledException();

            // Preserve existing contract: canceled requests resolve as timeout/no-fix (null).
            if (cancellationToken.IsCancellationRequested)
                return null;

            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            var listener = new SingleShotLocationListener(tcs, locationManager, desiredAccuracy, acceptableSource);

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
            bool hasRegisteredProvider = false;
            foreach (var provider in allProviders)
            {
                try
                {
                    locationManager.RequestLocationUpdates(provider, 0L, 0f, listener);
                    hasRegisteredProvider = true;
                }
                catch (Java.Lang.IllegalArgumentException)
                {
                    // Provider may disappear between enumeration and registration.
                }
            }

            if (!hasRegisteredProvider)
            {
                try { locationManager.RemoveUpdates(listener); } catch { /* ignore */ }
                throw new NoSuitableLocationProviderException();
            }

            // Register cancellation: remove the listener and complete with null when the
            // CancellationToken fires (e.g. the user-configured GpsReceiveTimeoutSec elapses or
            // the hard 10-minute limit is reached).
            using var registration = effectiveToken.Register(() =>
            {
                try { locationManager.RemoveUpdates(listener); } catch { /* ignore – listener may already be unregistered */ }
                CompleteWhenNoAcceptableFix();
            });

            // If cancellation happened between request and registration, complete deterministically.
            if (effectiveToken.IsCancellationRequested)
            {
                try { locationManager.RemoveUpdates(listener); } catch { /* ignore */ }
                CompleteWhenNoAcceptableFix();
            }

            return await tcs.Task.ConfigureAwait(false);

            // When the wait ends without a preferred (GPS-provider) fix, fall back to the best
            // acceptable lower-priority fix seen (e.g. a fast network/fused fix) so a coordinate is
            // still captured with its true source. If the only fixes seen were refused by the
            // acceptance policy (e.g. mock locations), surface a restricted-source error; otherwise
            // resolve as timeout/no-fix (null) to preserve the existing contract.
            void CompleteWhenNoAcceptableFix()
            {
                var fallback = listener.BestFallbackLocation;
                if (fallback != null)
                    tcs.TrySetResult(fallback);
                else if (listener.RejectedRestrictedFix)
                    tcs.TrySetException(new RestrictedLocationSourceException());
                else
                    tcs.TrySetResult(null);
            }
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
        internal sealed class SingleShotLocationListener : Java.Lang.Object, ILocationListener
        {
            private readonly TaskCompletionSource<GpsLocation> tcs;
            private readonly LocationManager locationManager;
            private readonly double desiredAccuracy;
            private readonly AcceptableGpsLocationSource acceptableSource;

            // Set when a received fix was rejected because it violates the acceptance policy
            // (e.g. a mock location, or a non-GPS provider in a GPS-only mode). Used to surface a
            // restricted-source error instead of a generic timeout when no acceptable fix arrives.
            private volatile bool rejectedRestrictedFix;

            internal bool RejectedRestrictedFix => this.rejectedRestrictedFix;

            // Best acceptable but lower-priority fix seen so far (e.g. a fast network/fused fix, or a
            // coarse GPS fix). A preferred GPS-provider fix wins immediately; otherwise this fallback
            // is returned on timeout so a coordinate is still captured with its true source instead of
            // a fast-but-coarse provider being reported when a better GPS fix would have arrived.
            private readonly object fallbackSync = new object();
            private GpsLocation bestFallbackLocation;

            // Monotonic clock reading taken when the request started. Location providers may deliver a
            // cached fix obtained before the request; such a fix describes an earlier source (typically
            // an older built-in GPS fix) and would mask the source actually in use now.
            private readonly long requestStartElapsedRealtimeNanos = SystemClock.ElapsedRealtimeNanos();

            // Cached fixes acquired shortly before the request are still current, so allow a small margin.
            // Five seconds covers a fix captured while the interviewer was opening the question (the
            // typical provider update interval is one second) without admitting older stored fixes.
            private const long AcceptableFixAgeNanos = 5L * 1000 * 1000 * 1000;

            internal GpsLocation BestFallbackLocation
            {
                get { lock (this.fallbackSync) return this.bestFallbackLocation; }
            }

            internal SingleShotLocationListener(
                TaskCompletionSource<GpsLocation> tcs,
                LocationManager locationManager,
                double desiredAccuracy,
                AcceptableGpsLocationSource acceptableSource)
            {
                this.tcs = tcs;
                this.locationManager = locationManager;
                this.desiredAccuracy = desiredAccuracy;
                this.acceptableSource = acceptableSource;
            }

            public void OnLocationChanged(AndroidLocation location)
            {
                // Enforce the workspace-configured acceptance criteria: reject fixes that do not
                // come from the required provider, or that are mock when mock is not permitted.
                bool isFromGpsProvider = location.Provider == LocationManager.GpsProvider;
                bool isFromMockProvider = location.IsMockLocation();

                // Ignore fixes cached by the provider before this request started: they report the
                // source that produced them earlier (e.g. a stored built-in GPS fix) and would be
                // captured and labelled in paradata instead of the location source in use now.
                // Mock fixes are exempt: external GPS sensors inject under the gps provider as mock
                // fixes and their ElapsedRealtimeNanos may not align with the device clock, so the
                // age filter must not be applied to them.
                if (!isFromMockProvider && IsCachedFromBeforeRequest(location))
                    return;
                if (!this.acceptableSource.IsLocationAcceptable(isFromGpsProvider, isFromMockProvider))
                {
                    // Remember that a fix arrived but was refused by the acceptance policy so the
                    // caller can report a restricted-source error rather than a plain timeout.
                    this.rejectedRestrictedFix = true;
                    return;
                }

                // External GPS devices may emit valid fixes whose Time (wall clock) does not align
                // with the device clock. Do not reject by that timestamp.

                var timestamp = GetTimestamp(location);
                var gpsLocation = new GpsLocation(
                    location.HasAccuracy ? location.Accuracy : null,
                    location.HasAltitude ? location.Altitude : null,
                    location.Latitude,
                    location.Longitude,
                    timestamp,
                    location.Provider,
                    isFromMockProvider);

                // A "preferred" fix comes from the GPS provider (built-in, or an external GPS sensor
                // injecting under the gps provider) — the correct, high-quality source. Built-in GPS
                // fixes must also meet the desired accuracy; external (mock) GPS accuracy is not
                // satellite-comparable, so it is exempt from that satellite-oriented threshold.
                // Return a preferred fix immediately.
                bool meetsDesiredAccuracy =
                    !(this.desiredAccuracy > 0 && location.HasAccuracy && location.Accuracy > this.desiredAccuracy);
                bool isPreferred = isFromGpsProvider && (isFromMockProvider || meetsDesiredAccuracy);

                if (!isPreferred)
                {
                    // Lower-priority acceptable fixes (non-GPS providers such as network/fused/WiFi,
                    // permitted in modes A/N) are captured but do not complete the request immediately:
                    // keep waiting so a subsequent GPS-provider fix — the correct source — can win and
                    // be shown in the result. Retain the most accurate such fix as a fallback so a
                    // coordinate is still returned on timeout. Coarse built-in GPS fixes keep waiting
                    // for a better satellite fix and are not recorded as a fallback.
                    if (!isFromGpsProvider && (isFromMockProvider || meetsDesiredAccuracy))
                        RecordFallback(gpsLocation);
                    return;
                }

                // Set result before removing updates so the task always completes even if
                // RemoveUpdates throws (e.g. when called from a non-looper thread).
                tcs.TrySetResult(gpsLocation);

                // Unregister so we act as a one-shot listener.
                try { locationManager.RemoveUpdates(this); } catch { /* ignore – result already set */ }
            }

            // A fix stamped on the device monotonic clock before this request started was cached by the
            // provider and does not describe the location source currently producing fixes. External
            // GPS sensors may inject fixes without a usable monotonic timestamp (zero or negative);
            // those are treated as current so that external sensors keep working.
            private bool IsCachedFromBeforeRequest(AndroidLocation location)
            {
                var fixElapsedRealtimeNanos = location.ElapsedRealtimeNanos;
                if (fixElapsedRealtimeNanos <= 0)
                    return false;

                return fixElapsedRealtimeNanos + AcceptableFixAgeNanos < this.requestStartElapsedRealtimeNanos;
            }

            // Keep the most accurate acceptable lower-priority fix. Fixes without a reported accuracy
            // rank worst, so a fix that reports accuracy is always preferred over one that does not.
            private void RecordFallback(GpsLocation candidate)
            {
                lock (this.fallbackSync)
                {
                    if (this.bestFallbackLocation == null
                        || (candidate.Accuracy.HasValue
                            && (!this.bestFallbackLocation.Accuracy.HasValue
                                || candidate.Accuracy.Value < this.bestFallbackLocation.Accuracy.Value)))
                    {
                        this.bestFallbackLocation = candidate;
                    }
                }
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
