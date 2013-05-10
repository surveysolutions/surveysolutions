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

    using DataEntryClient.SycProcess.Interfaces;

    /// <summary>
    /// The i sync process factory.
    /// </summary>
    public interface ISyncProcessFactory
    {
        #region Public Methods and Operators

        IWirelessSyncProcess GetNetworkProcess(Guid syncKey);
        IUsbSyncProcess GetUsbProcess(Guid syncKey);
        IEventSyncProcess GetRestProcess(Guid syncKey, Guid userId);

        #endregion
    }
}