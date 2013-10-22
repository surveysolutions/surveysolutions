using System;

namespace Main.Core.Export
{
    /// <summary>
    /// The DataExport interface.
    /// </summary>
    public interface IDataExport
    {
        #region Public Methods and Operators

        /// <summary>
        /// The export data.
        /// </summary>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        byte[] ExportData(Guid templateGuid, string type);

        #endregion
    }
}