namespace Main.Core.Documents
{
    using System;

    /// <summary>
    /// The location document.
    /// </summary>
    public class LocationDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationDocument"/> class.
        /// </summary>
        public LocationDocument()
        {
            this.CreationDate = DateTime.UtcNow;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; }

        #endregion
    }
}