// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncProcessFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The i sync process factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.SycProcessFactory
{
    using System;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;

    /// <summary>
    /// The i sync process factory.
    /// </summary>
    public interface ISyncProcessFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get process.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <param name="parentSyncKey">
        /// The parent sync key.
        /// </param>
        /// <returns>
        /// Sync procces object
        /// </returns>
        ISyncProcess GetProcess(SyncProcessType type, Guid syncKey, Guid? parentSyncKey);

        #endregion
    }
}