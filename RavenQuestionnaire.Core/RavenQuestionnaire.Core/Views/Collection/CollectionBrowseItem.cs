// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Collection
{
    /// <summary>
    /// The collection browse item.
    /// </summary>
    public class CollectionBrowseItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBrowseItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        public CollectionBrowseItem(string id, string title)
        {
            this.Id = id;
            this.Name = title;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        #endregion
    }
}