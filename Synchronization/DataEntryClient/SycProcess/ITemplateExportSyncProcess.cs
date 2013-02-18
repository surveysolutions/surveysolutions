// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITemplateExportSyncProcess.cs" company="">
//   
// </copyright>
// <summary>
//   The TemplateExportSyncProcess interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.SycProcess
{
    using System;

    using DataEntryClient.SycProcess.Interfaces;

    /// <summary>
    /// The TemplateExportSyncProcess interface.
    /// </summary>
    public interface ITemplateExportSyncProcess : ISyncProcess
    {
        #region Public Methods and Operators

        /// <summary>
        /// The export
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="templateGuid">
        /// The template Guid.
        /// </param>
        /// <param name="clientGuid">
        /// The client Guid.
        /// </param>
        /// <returns>
        /// Zip file as byte array
        /// </returns>
        byte[] Export(string syncProcessDescription, Guid? templateGuid, Guid? clientGuid);

        #endregion
    }
}