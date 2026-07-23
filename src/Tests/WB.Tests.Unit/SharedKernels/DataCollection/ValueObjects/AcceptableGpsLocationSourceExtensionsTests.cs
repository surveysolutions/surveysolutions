using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueObjects
{
    [TestOf(typeof(AcceptableGpsLocationSourceExtensions))]
    public class AcceptableGpsLocationSourceExtensionsTests
    {
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, true)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, true)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, false)]
        [TestCase(AcceptableGpsLocationSource.Any, false)]
        public void when_checking_RequiresGpsProvider_should_return_expected(AcceptableGpsLocationSource source, bool expected)
        {
            Assert.That(source.RequiresGpsProvider(), Is.EqualTo(expected));
        }

        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, false)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, true)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, false)]
        [TestCase(AcceptableGpsLocationSource.Any, true)]
        public void when_checking_AllowsMockProvider_should_return_expected(AcceptableGpsLocationSource source, bool expected)
        {
            Assert.That(source.AllowsMockProvider(), Is.EqualTo(expected));
        }

        // B: built-in GPS only, no mock.
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, true, false, true)]
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, true, true, false)]
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, false, false, false)]
        [TestCase(AcceptableGpsLocationSource.BuiltInGpsOnly, false, true, false)]
        // E: GPS provider, mock allowed (external GPS sensors).
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, true, false, true)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, true, true, true)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, false, false, false)]
        [TestCase(AcceptableGpsLocationSource.BuiltInOrExternalGps, false, true, false)]
        // A: any provider, no mock.
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, true, false, true)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, false, false, true)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, true, true, false)]
        [TestCase(AcceptableGpsLocationSource.AnyNonMock, false, true, false)]
        // N: anything permitted.
        [TestCase(AcceptableGpsLocationSource.Any, true, false, true)]
        [TestCase(AcceptableGpsLocationSource.Any, false, false, true)]
        [TestCase(AcceptableGpsLocationSource.Any, true, true, true)]
        [TestCase(AcceptableGpsLocationSource.Any, false, true, true)]
        public void when_checking_IsLocationAcceptable_should_return_expected(
            AcceptableGpsLocationSource source, bool isFromGpsProvider, bool isFromMockProvider, bool expected)
        {
            Assert.That(source.IsLocationAcceptable(isFromGpsProvider, isFromMockProvider), Is.EqualTo(expected));
        }
    }
}
