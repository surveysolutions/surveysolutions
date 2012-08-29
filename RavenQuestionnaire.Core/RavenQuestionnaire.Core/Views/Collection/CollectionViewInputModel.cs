// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionViewInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection view input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Collection
{
    /// <summary>
    /// The collection view input model.
    /// </summary>
    public class CollectionViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionViewInputModel"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        public CollectionViewInputModel(string collectionId)
        {
            this.CollectionId = collectionId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public string CollectionId { get; set; }

        #endregion
    }
}