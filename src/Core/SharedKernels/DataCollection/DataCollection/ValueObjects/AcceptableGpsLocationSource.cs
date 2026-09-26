namespace WB.Core.SharedKernels.DataCollection.ValueObjects
{
    /// <summary>
    /// Controls which sources of a measured location are acceptable when answering a GPS question,
    /// and when recording geo-tracking/geofencing fixes, in the Interviewer App. When a workspace has
    /// not configured a value, the policy default is <see cref="BuiltInGpsOnly"/> (built-in GPS sensor
    /// only, no external or mock locations). <see cref="Any"/> preserves the historical behaviour where
    /// any location (including mock locations) is accepted.
    /// </summary>
    public enum AcceptableGpsLocationSource
    {
        /// <summary>
        /// N: Anything is permitted, without any refusal from Survey Solutions
        /// (any provider, mock permitted).
        /// </summary>
        Any = 0,

        /// <summary>
        /// A: Anything that is not mock is permitted (any provider - WiFi, fusion, GPS, ... - but no mock locations).
        /// </summary>
        AnyNonMock = 1,

        /// <summary>
        /// E: Built-in GPS is permitted and external GPS (exposed on Android as a mock provider) is permitted.
        /// Only the GPS provider is accepted, but mock locations reported by that provider are allowed.
        /// </summary>
        BuiltInOrExternalGps = 2,

        /// <summary>
        /// B: Built-in location sensor only - only the GPS provider, no external GPS, no mock locations permitted.
        /// </summary>
        BuiltInGpsOnly = 3,
    }
}
