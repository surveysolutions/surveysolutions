// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionItemBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection item browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    /// <summary>
    /// The collection item browse item.
    /// </summary>
    public class CollectionItemBrowseItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemBrowseItem"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public CollectionItemBrowseItem(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; private set; }

        #endregion
    }
}