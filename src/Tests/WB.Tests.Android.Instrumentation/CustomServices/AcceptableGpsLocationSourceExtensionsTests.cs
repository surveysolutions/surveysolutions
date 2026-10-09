using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    [TestFixture]
    public class AcceptableGpsLocationSourceExtensionsTests
    {
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, true)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, true)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, false)]
        [TestCase(AcceptableGpsLocationSource.Any, false)]
        public void RequiresGpsProvider_returns_expected(AcceptableGpsLocationSource source, bool expected)
        {
            Assert.That(source.RequiresGpsProvider(), Is.EqualTo(expected));
        }

        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, true)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, false)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, false)]
        [TestCase(AcceptableGpsLocationSource.Any, false)]
        public void RequiresEnabledGpsProvider_returns_expected(AcceptableGpsLocationSource source, bool expected)
        {
            Assert.That(source.RequiresEnabledGpsProvider(), Is.EqualTo(expected));
        }

        [Test]
        public void IsLocationAcceptable_rejects_unknown_enum_values()
        {
            var unknownSource = (AcceptableGpsLocationSource)999;

            Assert.That(unknownSource.IsLocationAcceptable(isFromGpsProvider: true, isFromMockProvider: false), Is.False);
            Assert.That(unknownSource.IsLocationAcceptable(isFromGpsProvider: false, isFromMockProvider: false), Is.False);
            Assert.That(unknownSource.IsLocationAcceptable(isFromGpsProvider: true, isFromMockProvider: true), Is.False);
        }

        [Test]
        public void BuiltInOrExternalGps_accepts_only_gps_provider_and_allows_mock()
        {
            var source = AcceptableGpsLocationSource.BuiltInOrExternalGps;

            Assert.That(source.IsLocationAcceptable(isFromGpsProvider: true, isFromMockProvider: false), Is.True);
            Assert.That(source.IsLocationAcceptable(isFromGpsProvider: true, isFromMockProvider: true), Is.True);
            Assert.That(source.IsLocationAcceptable(isFromGpsProvider: false, isFromMockProvider: false), Is.False);
        }
    }
}
