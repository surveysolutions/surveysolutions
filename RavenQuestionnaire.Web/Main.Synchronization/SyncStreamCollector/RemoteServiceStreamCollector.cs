// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteServiceStreamCollector.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The remote service stream collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncStreamCollector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;
    

    using Ninject;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// The remote service stream collector.
    /// </summary>
    public class RemoteServiceStreamCollector : ISyncStreamCollector
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

        /// <summary>
        /// The process Guid.
        /// </summary>
        private readonly Guid processGuid;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServiceStreamCollector"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="baseAddress">
        /// The base address.
        /// </param>
        public RemoteServiceStreamCollector(IKernel kernel, Guid processGuid, string baseAddress)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
            this.baseAddress = baseAddress;
            this.processGuid = processGuid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        public int MaxChunkSize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether support sync stat.
        /// </summary>
        public bool SupportSyncStat
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The collect.
        /// </summary>
        /// <param name="chunk">
        /// The chunk.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool Collect(IEnumerable<AggregateRootEvent> chunk)
        {
            this.chanelFactoryWrapper.Execute<IEventPipe>(
                this.baseAddress, (client) => this.ProcessEvents(client, chunk));
            return true;
        }

        
        /// <summary>
        /// The finish.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Finish()
        {

        }

        /// <summary>
        /// The get stat.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public List<UserSyncProcessStatistics> GetStat()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The prepare to collect.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void PrepareToCollect()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process list of events
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="chunk">
        /// The chunk.
        /// </param>
        /// <returns>
        /// The process events.
        /// </returns>
        protected ErrorCodes ProcessEvents(IEventPipe client, IEnumerable<AggregateRootEvent> chunk)
        {
            // this.Invoker.Execute(new PushEventsCommand(this.ProcessGuid, events));
            var message = new EventSyncMessage { Command = chunk.ToArray(), SynchronizationKey = this.processGuid };
            return client.Process(message);
        }

        #endregion
    }
}