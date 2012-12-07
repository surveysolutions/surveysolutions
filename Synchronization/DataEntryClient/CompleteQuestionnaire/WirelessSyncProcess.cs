// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WirelessSyncProcess.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.WcfInfrastructure;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Ninject;

    using NLog;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.Handshake;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public class WirelessSyncProcess : AbstractSyncProcess
    {
        #region Constants and Fields

        /// <summary>
        /// The base adress.
        /// </summary>
        private readonly string baseAdress;

        /// <summary>
        /// The chanel factory wrapper.
        /// </summary>
        private readonly IChanelFactoryWrapper chanelFactoryWrapper;

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
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        public WirelessSyncProcess(IKernel kernel, Guid syncProcess, string baseAdress)
            : base(kernel, syncProcess)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
            this.baseAdress = baseAdress;
        }

        #endregion

        #region Public Methods and Operators

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
            this.chanelFactoryWrapper.Execute<IGetLastSyncEvent>(this.baseAdress, (client) => { result = client.Process(clientKey); });
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event exporter
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        protected override void ExportEvents(Guid syncKey)
        {
            // TODO: uncomment that string if we'll be synchronizing delta instead of everything   Guid? lastSyncEventGuid = GetLastSyncEventGuid(syncKey);
            this.chanelFactoryWrapper.Execute<IEventPipe>(
                this.baseAdress, (client) => this.ProcessEvents(syncKey, client));
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
                this.baseAdress, (client) => { result = client.Process(); });
            if (result == null)
            {
                throw new Exception("aggregate roots list is empty");
            }

            this.Invoker.Execute(new PushEventsCommand(this.ProcessGuid, result.Roots));
            var events = new List<AggregateRootEvent>();
            this.chanelFactoryWrapper.Execute<IGetEventStream>(
                this.baseAdress, 
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
                                Logger logger = LogManager.GetCurrentClassLogger();
                                logger.Fatal("Import error", ex);

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