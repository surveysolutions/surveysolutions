using Android.Locations;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.UI.Shared.Enumerator.Services;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    [TestFixture]
    public class GeolocationBackgroundServiceTests
    {
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps)]
        public void when_mode_requires_gps_should_register_only_gps_provider(AcceptableGpsLocationSource source)
        {
            var providers = GeolocationBackgroundService.GetProvidersForSource(
                source, new[] { LocationManager.NetworkProvider, "external" });

            Assert.That(providers, Is.EqualTo(new[] { LocationManager.GpsProvider }));
        }

        [TestCase(AcceptableGpsLocationSource.AnyNonMock)]
        [TestCase(AcceptableGpsLocationSource.Any)]
        public void when_mode_allows_any_provider_should_register_all_enabled_providers(
            AcceptableGpsLocationSource source)
        {
            var enabledProviders = new[] { LocationManager.NetworkProvider, "external" };
            var providers = GeolocationBackgroundService.GetProvidersForSource(source, enabledProviders);

            Assert.That(providers, Is.EqualTo(
                new[] { LocationManager.NetworkProvider, "external", LocationManager.GpsProvider }));
        }
    }
}
