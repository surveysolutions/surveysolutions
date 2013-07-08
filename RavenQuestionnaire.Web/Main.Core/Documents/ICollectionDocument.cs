namespace Main.Core.Documents
{
    /// <summary>
    /// The CollectionDocument interface.
    /// </summary>
    public interface ICollectionDocument
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        #endregion
    }
}