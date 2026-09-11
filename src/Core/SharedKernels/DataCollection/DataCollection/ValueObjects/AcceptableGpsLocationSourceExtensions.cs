namespace WB.Core.SharedKernels.DataCollection.ValueObjects
{
    /// <summary>
    /// Shared acceptance rules for <see cref="AcceptableGpsLocationSource"/> so that answering GPS
    /// questions, geo-tracking and geofencing on the Interviewer tablet all enforce the workspace
    /// policy identically.
    /// </summary>
    public static class AcceptableGpsLocationSourceExtensions
    {
        public static bool IsKnownValue(this AcceptableGpsLocationSource source) =>
            source is AcceptableGpsLocationSource.Any
                or AcceptableGpsLocationSource.AnyNonMock
                or AcceptableGpsLocationSource.BuiltInOrExternalGps
                or AcceptableGpsLocationSource.BuiltInGpsOnly;

        /// <summary>
        /// Modes B (<see cref="AcceptableGpsLocationSource.BuiltInGpsOnly"/>) and
        /// E (<see cref="AcceptableGpsLocationSource.BuiltInOrExternalGps"/>) demand the physical GPS provider.
        /// Modes A and N accept any provider.
        /// </summary>
        public static bool RequiresGpsProvider(this AcceptableGpsLocationSource source) => source switch
        {
            AcceptableGpsLocationSource.BuiltInGpsOnly => true,
            AcceptableGpsLocationSource.BuiltInOrExternalGps => true,
            AcceptableGpsLocationSource.AnyNonMock => false,
            AcceptableGpsLocationSource.Any => false,
            _ => false
        };

        /// <summary>
        /// Only mode B (<see cref="AcceptableGpsLocationSource.BuiltInGpsOnly"/>) requires
        /// the hardware GPS provider to be enabled.
        /// </summary>
        public static bool RequiresEnabledGpsProvider(this AcceptableGpsLocationSource source) => source switch
        {
            AcceptableGpsLocationSource.BuiltInGpsOnly => true,
            AcceptableGpsLocationSource.BuiltInOrExternalGps => false,
            AcceptableGpsLocationSource.AnyNonMock => false,
            AcceptableGpsLocationSource.Any => false,
            _ => false
        };

        /// <summary>
        /// Mock locations (external Bluetooth/USB GPS sensors are exposed this way on Android) are only
        /// permitted in modes E (<see cref="AcceptableGpsLocationSource.BuiltInOrExternalGps"/>) and
        /// N (<see cref="AcceptableGpsLocationSource.Any"/>).
        /// </summary>
        public static bool AllowsMockProvider(this AcceptableGpsLocationSource source) => source switch
        {
            AcceptableGpsLocationSource.BuiltInGpsOnly => false,
            AcceptableGpsLocationSource.BuiltInOrExternalGps => true,
            AcceptableGpsLocationSource.AnyNonMock => false,
            AcceptableGpsLocationSource.Any => true,
            _ => false
        };

        /// <summary>
        /// Returns <c>true</c> when a received fix satisfies the workspace-configured acceptance criteria:
        /// it must come from the GPS provider when the mode requires it, and must not be a mock fix when
        /// mock locations are not permitted.
        /// </summary>
        public static bool IsLocationAcceptable(this AcceptableGpsLocationSource source,
            bool isFromGpsProvider, bool isFromMockProvider)
        {
            if (!source.IsKnownValue())
                return false;

            if (source.RequiresGpsProvider() && !isFromGpsProvider)
                return false;

            if (!source.AllowsMockProvider() && isFromMockProvider)
                return false;

            return true;
        }
    }
}
