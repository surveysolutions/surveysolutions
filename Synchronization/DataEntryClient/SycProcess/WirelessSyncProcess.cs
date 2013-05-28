// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WirelessSyncProcess.cs" company="">
//   
// </copyright>
// <summary>
//   The complete questionnaire sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.SharedKernel.Utils.Logging;

namespace DataEntryClient.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.SycProcess.Interfaces;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Ninject;
    
    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.Synchronization;
    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public class WirelessSyncProcess : AbstractSyncProcess, IWirelessSyncProcess
    {
        #region Fields

        /// <summary>
        /// The chanel factory wrapper.
        /// </summary>
        private readonly IChanelFactoryWrapper chanelFactoryWrapper;

        /// <summary>
        /// The base address.
        /// </summary>
        private string baseAddress;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WirelessSyncProcess"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="syncProcess">
        /// Sync process key
        /// </param>
        public WirelessSyncProcess(IKernel kernel, Guid syncProcess)
            : base(kernel, syncProcess)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <returns>
        /// Error Codes
        /// </returns>
        public ErrorCodes Export(string syncProcessDescription, string baseAdress)
        {
            this.baseAddress = baseAdress;
            return this.Export(syncProcessDescription);
        }

        /// <summary>
        /// The get last sync event guid.
        /// </summary>
        /// <param name="clientKey">
        /// The client key.
        /// </param>
        /// <returns>
        /// The Guid or null.
        /// </returns>
        public Guid? GetLastSyncEventGuid(Guid clientKey)
        {
            Guid? result = null;
            this.chanelFactoryWrapper.Execute<IGetLastSyncEvent>(
                this.baseAddress, (client) => { result = client.Process(clientKey); });
            return result;
        }

        /// <summary>
        /// The Import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <returns>
        /// Error Codes
        /// </returns>
        [Obsolete("Import(string) is deprecated, please use Import(string, string) instead.", true)]
        public ErrorCodes Import(string syncProcessDescription)
        {
            return ErrorCodes.Fail;
        }

        /// <summary>
        /// The Import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <returns>
        /// Error codes
        /// </returns>
        public ErrorCodes Import(string syncProcessDescription, string baseAdress)
        {
            this.baseAddress = baseAdress;
            return base.Import(syncProcessDescription);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event exporter
        /// </summary>
        protected override void ExportEvents()
        {
            this.chanelFactoryWrapper.Execute<IEventPipe>(this.baseAddress, (client) => this.ProcessEvents(client));
        }

        /// <summary>
        /// Gets uploaded list of events
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        protected override IEnumerable<AggregateRootEvent> GetEventStream()
        {
            ListOfAggregateRootsForImportMessage result = null;
            this.chanelFactoryWrapper.Execute<IGetAggragateRootList>(
                this.baseAddress, (client) => { result = client.Process(); });

            if (result == null)
            {
                throw new Exception("aggregate roots list is empty");
            }

            this.Invoker.Execute(new PushEventsCommand(this.ProcessGuid, result.Roots));
            var events = new List<AggregateRootEvent>();
            this.chanelFactoryWrapper.Execute<IGetEventStream>(
                this.baseAddress, 
                (client) =>
                    {
                        foreach (ProcessedEventChunk root in result.Roots)
                        {
                            try
                            {
                                if (root.EventKeys.Count == 0)
                                {
                                    continue;
                                }

                                AggregateRootEvent[] stream =
                                    client.Process(root.EventKeys.First(), root.EventKeys.Count).EventStream;
                                events.AddRange(stream);

                                this.Invoker.Execute(
                                    new ChangeEventStatusCommand(
                                        this.ProcessGuid, root.EventChunckPublicKey, EventState.Completed));
                            }
                            catch (Exception ex)
                            {
                                
                                LogManager.GetLogger(this.GetType()).Fatal("Import error", ex);

                                events = null;
                                this.Invoker.Execute(
                                    new ChangeEventStatusCommand(
                                        this.ProcessGuid, root.EventChunckPublicKey, EventState.Error));
                            }
                        }
                    });
            return events;
        }

        #endregion
    }
}