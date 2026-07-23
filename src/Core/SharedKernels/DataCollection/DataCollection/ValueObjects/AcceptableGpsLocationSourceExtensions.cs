namespace WB.Core.SharedKernels.DataCollection.ValueObjects
{
    /// <summary>
    /// Shared acceptance rules for <see cref="AcceptableGpsLocationSource"/> so that answering GPS
    /// questions, geo-tracking and geofencing on the Interviewer tablet all enforce the workspace
    /// policy identically.
    /// </summary>
    public static class AcceptableGpsLocationSourceExtensions
    {
        /// <summary>
        /// Modes B (<see cref="AcceptableGpsLocationSource.BuiltInGpsOnly"/>) and
        /// E (<see cref="AcceptableGpsLocationSource.BuiltInOrExternalGps"/>) demand the physical GPS provider.
        /// Modes A and N accept any provider.
        /// </summary>
        public static bool RequiresGpsProvider(this AcceptableGpsLocationSource source) =>
            source == AcceptableGpsLocationSource.BuiltInGpsOnly
            || source == AcceptableGpsLocationSource.BuiltInOrExternalGps;

        /// <summary>
        /// Mock locations (external Bluetooth/USB GPS sensors are exposed this way on Android) are only
        /// permitted in modes E (<see cref="AcceptableGpsLocationSource.BuiltInOrExternalGps"/>) and
        /// N (<see cref="AcceptableGpsLocationSource.Any"/>).
        /// </summary>
        public static bool AllowsMockProvider(this AcceptableGpsLocationSource source) =>
            source == AcceptableGpsLocationSource.Any
            || source == AcceptableGpsLocationSource.BuiltInOrExternalGps;

        /// <summary>
        /// Returns <c>true</c> when a received fix satisfies the workspace-configured acceptance criteria:
        /// it must come from the GPS provider when the mode requires it, and must not be a mock fix when
        /// mock locations are not permitted.
        /// </summary>
        public static bool IsLocationAcceptable(this AcceptableGpsLocationSource source,
            bool isFromGpsProvider, bool isFromMockProvider)
        {
            if (source.RequiresGpsProvider() && !isFromGpsProvider)
                return false;

            if (!source.AllowsMockProvider() && isFromMockProvider)
                return false;

            return true;
        }
    }
}
