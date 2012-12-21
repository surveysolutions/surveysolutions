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
        /// The AbstractSyncProcess
        /// </returns>
        public ISyncProcess GetProcess(SyncProcessType type, Guid syncKey, Guid? parentSyncKey)
        {
            switch (type)
            {
                case SyncProcessType.Usb:
                    return new UsbSyncProcess(this.kernel, syncKey);
                case SyncProcessType.Network:
                    return new WirelessSyncProcess(this.kernel, syncKey);
                case SyncProcessType.Template:
                    return new TemplateExportSyncProcess(this.kernel, syncKey);
                case SyncProcessType.Event:
                    return new EventSyncProcess(this.kernel, syncKey);
            }

            throw new Exception("Cannot creat sync process with type:" + type.ToString());
        }

        #endregion
    }
}