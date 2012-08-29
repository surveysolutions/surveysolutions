// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionItemBrowseInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection item browse input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    using System;

    /// <summary>
    /// The collection item browse input model.
    /// </summary>
    public class CollectionItemBrowseInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemBrowseInputModel"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <param name="QuestionId">
        /// The question id.
        /// </param>
        public CollectionItemBrowseInputModel(string collectionId, Guid QuestionId)
        {
            this.CollectionId = collectionId;

            this.QuestionId = QuestionId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// Gets or sets the question id.
        /// </summary>
        public Guid QuestionId { get; set; }

        #endregion
    }
}