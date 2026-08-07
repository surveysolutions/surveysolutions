using System.Reflection;
using Android.Locations;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    [TestFixture]
    public class GpsLocationServiceTests
    {
        [Test]
        public async Task when_acceptable_fix_received_should_capture_coordinates()
        {
            var tcs = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            var listenerType = Type.GetType(
                "WB.UI.Shared.Enumerator.Services.Internals.GpsLocationService+SingleShotLocationListener, WB.UI.Shared.Enumerator",
                throwOnError: false);
            Assert.That(listenerType, Is.Not.Null, "SingleShotLocationListener type was not found.");

            var listenerConstructor = listenerType!.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(TaskCompletionSource<GpsLocation>), typeof(LocationManager), typeof(double), typeof(AcceptableGpsLocationSource) },
                modifiers: null);

            Assert.That(listenerConstructor, Is.Not.Null,
                "SingleShotLocationListener constructor signature has changed.");

            var listener = (ILocationListener)listenerConstructor!.Invoke(
                new object?[] { tcs, null, -1d, AcceptableGpsLocationSource.AnyNonMock });

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
    }
}
