// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessFactory.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.SycProcessFactory
{
    using System;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;

    using Ninject;

    /// <summary>
    /// Sync Process Factory
    /// </summary>
    public class SyncProcessFactory : ISyncProcessFactory
    {
        /// <summary>
        /// The kernel
        /// </summary>
        private readonly IKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessFactory"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public SyncProcessFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IWirelessSyncProcess GetNetworkProcess(Guid syncKey)
        {
            return new WirelessSyncProcess(this.kernel, syncKey);
        }

        public IUsbSyncProcess GetUsbProcess(Guid syncKey)
        {
            return new UsbSyncProcess(this.kernel, syncKey);
        }

        public IEventSyncProcess GetRestProcess(Guid syncKey, Guid userId)
        {
            return new EventSyncProcess(this.kernel, syncKey, userId);
        }
    }
}