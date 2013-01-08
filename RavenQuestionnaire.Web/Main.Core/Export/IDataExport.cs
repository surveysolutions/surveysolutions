// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataExport.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Export
{
    using System;

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