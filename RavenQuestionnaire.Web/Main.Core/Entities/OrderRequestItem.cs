namespace Main.Core.Entities
{
    /// <summary>
    /// The order request item.
    /// </summary>
    public class OrderRequestItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public OrderDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Json serialization
        /// </summary>
        /// <returns>
        /// Serialized object
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{\"Direction\": \"{0}\", \"Field\": \"{1}\" }}", this.Direction, this.Field);
        }
        #endregion
    }
}