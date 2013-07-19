namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The location with user statistic.
    /// </summary>
    public class LocationWithUserStatistic
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the user count.
        /// </summary>
        public int UserCount { get; set; }

        #endregion
    }
}