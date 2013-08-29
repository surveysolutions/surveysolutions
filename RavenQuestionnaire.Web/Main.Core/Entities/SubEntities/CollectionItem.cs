namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The collection item.
    /// </summary>
    public class CollectionItem : ICollectionItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItem"/> class.
        /// </summary>
        public CollectionItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItem"/> class.
        /// </summary>
        /// <param name="_id">
        /// The _id.
        /// </param>
        /// <param name="_key">
        /// The _key.
        /// </param>
        /// <param name="_value">
        /// The _value.
        /// </param>
        public CollectionItem(Guid _id, string _key, string _value)
        {
            this.PublicKey = _id;
            this.Key = _key;
            this.Value = _value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        #endregion
    }
}