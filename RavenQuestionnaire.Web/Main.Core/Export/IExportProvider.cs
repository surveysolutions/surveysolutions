// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExportProvider.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ExportProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;

namespace Main.Core.Export
{
    /// <summary>
    /// The ExportProvider interface.
    /// </summary>
    public interface IExportProvider<T>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The do export.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
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
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <returns>
        /// The System.IO.Stream.
        /// </returns>
        Stream DoExportToStream(T items);

        #endregion
    }
}