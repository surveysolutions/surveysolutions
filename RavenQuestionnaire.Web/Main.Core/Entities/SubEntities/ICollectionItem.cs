namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The CollectionItem interface.
    /// </summary>
    public interface ICollectionItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        string Value { get; set; }

        #endregion
    }
}