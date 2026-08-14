using Android.Locations;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    [TestFixture]
    public class GpsLocationServiceTests
    {
        [Test]
        public async Task when_acceptable_fix_received_should_capture_coordinates()
        {
            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            ILocationListener listener = new GpsLocationService.SingleShotLocationListener(
                tcs, locationManager: null, desiredAccuracy: -1d, acceptableSource: AcceptableGpsLocationSource.AnyNonMock);

            var androidLocation = new Location(LocationManager.GpsProvider)
            {
                Latitude = 49.842957d,
                Longitude = 24.031111d,
                Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            listener.OnLocationChanged(androidLocation);

            var gpsLocation = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

            Assert.That(gpsLocation, Is.Not.Null);
            Assert.That(gpsLocation!.Latitude, Is.EqualTo(49.842957d));
            Assert.That(gpsLocation.Longitude, Is.EqualTo(24.031111d));
            Assert.That(gpsLocation.Provider, Is.EqualTo(LocationManager.GpsProvider));
            Assert.That(gpsLocation.IsFromMockProvider, Is.False);
        }

        [Test]
        public async Task when_non_gps_coarse_fix_received_in_any_non_mock_mode_should_capture_coordinates_as_fallback()
        {
            // Mode A (AnyNonMock) accepts WiFi/network/fused fixes, but a fast coarse fix must not be
            // reported immediately as the result — it is retained as a fallback so a subsequent
            // GPS-provider fix (the correct source) can win. The fallback is returned on timeout so a
            // coordinate is still captured when no GPS fix arrives.
            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            var listener = new GpsLocationService.SingleShotLocationListener(
                tcs, locationManager: null, desiredAccuracy: 10d, acceptableSource: AcceptableGpsLocationSource.AnyNonMock);

            var androidLocation = new Location(LocationManager.NetworkProvider)
            {
                Latitude = 49.842957d,
                Longitude = 24.031111d,
                Accuracy = 250f,
                Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            ((ILocationListener)listener).OnLocationChanged(androidLocation);

            await Task.Yield();

            Assert.That(tcs.Task.IsCompleted, Is.False);
            Assert.That(listener.BestFallbackLocation, Is.Not.Null);
            Assert.That(listener.BestFallbackLocation!.Provider, Is.EqualTo(LocationManager.NetworkProvider));
        }

        [Test]
        public async Task when_coarse_fix_precedes_accurate_gps_fix_should_report_gps_source()
        {
            // A fast coarse network fix arrives first, then an accurate GPS fix. The result must show
            // the GPS-provider coordinates, not the earlier coarse network fix.
            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            ILocationListener listener = new GpsLocationService.SingleShotLocationListener(
                tcs, locationManager: null, desiredAccuracy: 10d, acceptableSource: AcceptableGpsLocationSource.AnyNonMock);

            listener.OnLocationChanged(new Location(LocationManager.NetworkProvider)
            {
                Latitude = 1d,
                Longitude = 2d,
                Accuracy = 250f,
                Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            });

            listener.OnLocationChanged(new Location(LocationManager.GpsProvider)
            {
                Latitude = 49.842957d,
                Longitude = 24.031111d,
                Accuracy = 5f,
                Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            });

            var gpsLocation = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

            Assert.That(gpsLocation, Is.Not.Null);
            Assert.That(gpsLocation!.Provider, Is.EqualTo(LocationManager.GpsProvider));
            Assert.That(gpsLocation.Latitude, Is.EqualTo(49.842957d));
        }

        [Test]
        public void when_gps_fix_worse_than_desired_accuracy_should_not_capture_coordinates()
        {
            // Built-in GPS fixes are still held to the desired accuracy threshold.
            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            ILocationListener listener = new GpsLocationService.SingleShotLocationListener(
                tcs, locationManager: null, desiredAccuracy: 10d, acceptableSource: AcceptableGpsLocationSource.BuiltInGpsOnly);

            var androidLocation = new Location(LocationManager.GpsProvider)
            {
                Latitude = 49.842957d,
                Longitude = 24.031111d,
                Accuracy = 250f,
                Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            listener.OnLocationChanged(androidLocation);

            Assert.That(tcs.Task.IsCompleted, Is.False);
        }

        [Test]
        public void when_only_restricted_fix_received_should_flag_rejected_restricted_fix()
        {
            // A mock fix in a mode that forbids mocks must be refused, and the listener must record
            // that a fix was rejected so the caller can report a restricted-source error instead of
            // a generic timeout when no acceptable fix ever arrives.
            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            var listener = new GpsLocationService.SingleShotLocationListener(
                tcs, locationManager: null, desiredAccuracy: -1d, acceptableSource: AcceptableGpsLocationSource.AnyNonMock);

            var androidLocation = new Location(LocationManager.GpsProvider)
            {
                Latitude = 49.842957d,
                Longitude = 24.031111d,
                Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsFromMockProvider = true,
            };

            ((ILocationListener)listener).OnLocationChanged(androidLocation);

            Assert.That(tcs.Task.IsCompleted, Is.False);
            Assert.That(listener.RejectedRestrictedFix, Is.True);
        }
    }
}
