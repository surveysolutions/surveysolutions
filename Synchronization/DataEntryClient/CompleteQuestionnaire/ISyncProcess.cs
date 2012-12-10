// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncProcess.cs" company="">
//   
// </copyright>
// <summary>
//   The WirelessSyncProcess interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;

    /// <summary>
    /// The WirelessSyncProcess interface.
    /// </summary>
    public interface ISyncProcess
    {
        #region Public Methods and Operators

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        void Export(Guid syncKey);

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        void Import(Guid syncKey);

        #endregion
    }
}