// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The collection view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Collection
{
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Utility;
    using Main.Core.Entities.SubEntities;

    using RavenQuestionnaire.Core.Views.CollectionItem;

    /// <summary>
    /// The collection view.
    /// </summary>
    public class CollectionView
    {
        #region Fields

        /// <summary>
        /// The _collection id.
        /// </summary>
        private string _collectionId;

        /// <summary>
        /// The _items.
        /// </summary>
        private List<CollectionItemView> _items;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionView"/> class.
        /// </summary>
        public CollectionView()
        {
            this.Items = new List<CollectionItemView>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionView"/> class.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public CollectionView(ICollectionDocument collection)
        {
            this.CollectionId = collection.Id;
            this.Name = collection.Name;
            var cd = (CollectionDocument)collection;
            this.Items = new List<CollectionItemView>();
            foreach (CollectionItem item in cd.Items)
            {
                this.Items.Add(new CollectionItemView(cd.Id, new CollectionItem(item.PublicKey, item.Key, item.Value)));
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public string CollectionId
        {
            get
            {
                return IdUtil.ParseId(this._collectionId);
            }

            set
            {
                this._collectionId = value;
            }
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CollectionItemView> Items
        {
            get
            {
                return this._items;
            }

            set
            {
                this._items = value;
                if (this._items == null)
                {
                    this._items = new List<CollectionItemView>();
                    return;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}