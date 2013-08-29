using System.IO;

namespace Main.Core.Export
{
    /// <summary>
    /// The export manager.
    /// </summary>
    public class ExportManager<T>
    {
        #region Fields

        /// <summary>
        /// The _provider.
        /// </summary>
        private readonly IExportProvider<T> _provider;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportManager"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public ExportManager(IExportProvider<T> provider)
        {
            this._provider = provider;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export.
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
        public bool Export(T items,string fileName)
        {
            this._provider.DoExport(items, fileName);
            return true;
        }

        /// <summary>
        /// The export to stream.
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
        public byte[] ExportToStream(T items)
        {
            return this._provider.DoExportToStream(items);
        }

        #endregion
    }
}