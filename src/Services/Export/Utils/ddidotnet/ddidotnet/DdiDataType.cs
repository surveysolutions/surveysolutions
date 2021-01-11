namespace ddidotnet
{
    /// <summary>
    ///     Type of data storage of individual variables.
    /// </summary>
    public enum DdiDataType
    {
        /// <summary>
        ///     Numeric type
        /// </summary>
        Numeric,

        /// <summary>
        ///     Fixed length string type
        /// </summary>
        FxString,

        /// <summary>
        ///     Dynamic length string type
        ///     \note It seems that the Nesstar Publisher v4.0.9 interprets fixed strings with width of 255 or more as dynamic
        ///     strings.
        /// </summary>
        DynString,

        /// <summary>
        ///     Date type
        ///     \note It seems that the Nesstar Publisher v4.0.9 interprets dynamic strings with width of 254 or less as fixed
        ///     strings.
        /// </summary>
        Date
    }
}
