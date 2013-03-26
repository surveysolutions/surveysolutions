// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteServiceEventStreamProvider.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The remote service event stream provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SyncSreamProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Ninject;

    //using NLog;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// The remote service event stream provider.
    /// </summary>
    public class RemoteServiceEventStreamProvider : IIntSyncEventStreamProvider
    {
        #region Fields

        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

        /// <summary>
        /// The chanel factory wrapper.
        /// </summary>
        private readonly IChanelFactoryWrapper chanelFactoryWrapper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServiceEventStreamProvider"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public RemoteServiceEventStreamProvider(IKernel kernel, Guid processGuid, string baseAddress)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
            this.ProcessGuid = processGuid;
            this.baseAddress = baseAddress;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the provider name.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return "Remote Service Reader";
            }
        }

        /// <summary>
        /// Gets the sync type.
        /// </summary>
        public SynchronizationType SyncType
        {
            get
            {
                return SynchronizationType.Pull;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        protected Guid ProcessGuid { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get event stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<AggregateRootEvent> GetEventStream()
        {
            return GetEventStreamInt();
        }

        /// <summary>
        /// The get total event count.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        public int? GetTotalEventCount()
        {
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets uploaded list of events
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        protected IEnumerable<AggregateRootEvent> GetEventStreamInt()
        {
            // TODO : change method to handle yield return of result to avoid storing all events in the memory

            ListOfAggregateRootsForImportMessage result = null;

            this.chanelFactoryWrapper.Execute<IGetAggragateRootList>(
                this.baseAddress, client => result = client.Process());

            if (result == null)
            {
                throw new Exception("Aggregate roots list is empty.");
            }


            // this.Invoker.Execute(new PushEventsCommand(this.ProcessGuid, result.Roots));

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

                                AggregateRootEvent[] stream = client.Process(root.EventKeys.First(), root.EventKeys.Count).EventStream;
                                
                                events.AddRange(stream);

                                /*this.Invoker.Execute(
                                    new ChangeEventStatusCommand(
                                        this.ProcessGuid, root.EventChunckPublicKey, EventState.Completed));*/
                            }
                            catch (Exception ex)
                            {
                                //Logger logger = LogManager.GetCurrentClassLogger();
                                //logger.Fatal("Import error", ex);

                                events = null;

                                /*this.Invoker.Execute(
                                    new ChangeEventStatusCommand(
                                        this.ProcessGuid, root.EventChunckPublicKey, EventState.Error));*/
                            }
                        }
                    });

            return events;
        }

        #endregion
    }
}