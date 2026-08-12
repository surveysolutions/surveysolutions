using Android.OS;
using AndroidLocation = Android.Locations.Location;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal static class AndroidLocationExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when the fix originates from a mock/injected provider.
        /// Combines <c>isMock()</c> (API 31+) with the deprecated <c>isFromMockProvider()</c> so mock
        /// locations are reliably detected across Android versions — on newer devices
        /// <c>isFromMockProvider()</c> can under-report fixes injected by a selected mock-location app,
        /// which would otherwise let a mock fix be accepted (and mislabeled) in mock-forbidding modes.
        /// </summary>
        public static bool IsMockLocation(this AndroidLocation location)
        {
            if (location == null)
                return false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S && location.IsMock)
                return true;

            return location.IsFromMockProvider;
        }
    }
}
