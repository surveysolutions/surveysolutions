// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Location.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The location.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using RavenQuestionnaire.Core.Documents;

    /// <summary>
    /// The location.
    /// </summary>
    public class Location : IEntity<LocationDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly LocationDocument innerDocument;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        public Location(string title)
        {
            this.innerDocument = new LocationDocument { Location = title };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="innerDocument">
        /// The inner document.
        /// </param>
        public Location(LocationDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the location id.
        /// </summary>
        public string LocationId
        {
            get
            {
                return this.innerDocument.Id;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.LocationDocument.
        /// </returns>
        public LocationDocument GetInnerDocument()
        {
            return this.innerDocument;
        }

        /// <summary>
        /// The update location.
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        public void UpdateLocation(string location)
        {
            this.innerDocument.Location = location;
        }

        #endregion
    }
}