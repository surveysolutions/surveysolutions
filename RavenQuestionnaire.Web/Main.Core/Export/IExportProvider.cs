namespace Main.Core.Export
{
    /// <summary>
    /// The ExportProvider interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IExportProvider<T>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The do export.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        bool DoExport(T items, string fileName);

        /// <summary>
        /// The do export to stream.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <returns>
        /// The System.IO.Stream.
        /// </returns>
        byte[] DoExportToStream(T items);

        #endregion
    }
}