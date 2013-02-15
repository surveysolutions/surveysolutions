// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncManager.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The sync manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncManager
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    //using NLog;

    //using LogManager = NLog.LogManager;

    /// <summary>
    /// The sync manager.
    /// </summary>
    public class SyncManager : ISyncManager
    {
        #region Constants

        /// <summary>
        /// The chunk size. Memory usage limitation.
        /// </summary>
        private const int ChunkSize = 1024;

        #endregion

        #region Fields

        /// <summary>
        /// The invoker.
        /// </summary>
        protected readonly ICommandService Invoker;

        /// <summary>
        /// The event stream provider.
        /// </summary>
        private readonly ISyncEventStreamProvider eventStreamProvider;

        /// <summary>
        /// The stream collector.
        /// </summary>
        private readonly ISyncStreamCollector streamCollector;

        /// <summary>
        /// The sync message.
        /// </summary>
        private readonly string syncMessage;

        /// <summary>
        /// The manager settings.
        /// </summary>
        private SyncManagerSettings managerSettings;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncManager"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="collector">
        /// The collector.
        /// </param>
        /// <param name="syncProcess">
        /// The sync process.
        /// </param>
        /// <param name="syncMessage">
        /// The sync Message.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public SyncManager(
            ISyncEventStreamProvider provider, 
            ISyncStreamCollector collector, 
            Guid syncProcess, 
            string syncMessage, 
            SyncManagerSettings settings)
        {
            this.eventStreamProvider = provider;
            this.managerSettings = settings;
            this.ProcessGuid = syncProcess;
            this.streamCollector = collector;

            this.syncMessage = syncMessage;
            this.Invoker = NcqrsEnvironment.Get<ICommandService>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the end time.
        /// </summary>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether is working.
        /// </summary>
        public bool IsWorking { get; private set; }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        public DateTime StartTime { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the process guid.
        /// </summary>
        protected Guid ProcessGuid { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get current progress.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public int? GetCurrentProgress()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The start pull.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartPull()
        {
            this.StartTime = DateTime.UtcNow;
            this.Pull();

            this.EndTime = DateTime.UtcNow;
        }

        /// <summary>
        /// The start push.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartPush()
        {
            this.StartTime = DateTime.UtcNow;

            this.Push();

            this.EndTime = DateTime.UtcNow;
        }

        /// <summary>
        /// The start synchronization.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartSynchronization()
        {
            /*this.StartTime = DateTime.UtcNow;
            this.Push();

            this.Pull();
            this.EndTime = DateTime.UtcNow;*/
        }

        /// <summary>
        /// The stop process.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StopProcess()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The process statistics.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        private void ProcessStatistics(AggregateRootEvent evnt)
        {
            // cteate statistics processor
            // to collect sync progress
        }

        /// <summary>
        /// The pull.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        private void Pull()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The push.
        /// </summary>
        private void Push()
        {
            this.Invoker.Execute(
                new CreateNewSynchronizationProcessCommand(
                    this.ProcessGuid, 
                    Guid.Empty, 
                    this.eventStreamProvider.SyncType, 
                    string.Format("{0}({1})", this.syncMessage, this.eventStreamProvider.ProviderName)));

            this.streamCollector.PrepareToCollect();

            int currentChunkSize = Math.Min(this.streamCollector.MaxChunkSize, ChunkSize);
            int counter = 0;

            var chunk = new List<AggregateRootEvent>();

            try
            {
                // read from stream and handle by chunk
                foreach (AggregateRootEvent evnt in this.eventStreamProvider.GetEventStream())
                {
                    this.ProcessStatistics(evnt);
                    chunk.Add(evnt);
                    counter++;

                    if (counter == currentChunkSize)
                    {
                        this.streamCollector.Collect(chunk);
                        chunk = new List<AggregateRootEvent>();
                        counter = 0;
                    }
                }

                // process partial completed chunk
                if (counter > 0)
                {
                    this.streamCollector.Collect(chunk);
                }

                // write stat for sync
                if (this.streamCollector.SupportSyncStat)
                {
                    this.Invoker.Execute(new PushStatisticsCommand(this.ProcessGuid, this.streamCollector.GetStat()));
                }

                // notify collector about finishing
                this.streamCollector.Finish();

                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed, "Ok"));
            }
            catch (Exception e)
            {
                //Logger logger = LogManager.GetCurrentClassLogger();
                //logger.Fatal("Import error", e);
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error, e.Message));
                throw;
                // return ErrorCodes.Fail;
            }
            finally
            {
            }
        }

        #endregion
    }
}