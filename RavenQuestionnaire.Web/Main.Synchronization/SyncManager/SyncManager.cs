// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncManager.cs" company="">
//   
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
        /// The status.
        /// </summary>
        private readonly SyncronizationStatus status;

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
        /// <param name="status">
        /// The status.
        /// </param>
        public SyncManager(
            ISyncEventStreamProvider provider, 
            ISyncStreamCollector collector, 
            Guid syncProcess, 
            string syncMessage, 
            SyncManagerSettings settings, 
            SyncronizationStatus status)
        {
            this.eventStreamProvider = provider;
            this.managerSettings = settings;
            this.ProcessGuid = syncProcess;
            this.streamCollector = collector;

            this.status = status;
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
        /// The start push.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartPump()
        {
            this.StartTime = DateTime.UtcNow;
            this.status.IsWorking = true;
            this.IsWorking = true;

            this.Pump();

            this.EndTime = DateTime.UtcNow;
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
        private void Pump()
        {
            try
            {
                this.status.Progress = 3;

                if (this.streamCollector == null)
                {
                    this.status.Result = false;
                    this.status.ErrorMessage = "Incorrect receiver.";
                    this.status.Progress = 98;
                }

                this.status.Progress++;
                this.status.CurrentStageDescription = "Stage is starting.";

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

                this.status.Progress = 15;
                this.status.CurrentStageDescription = "Process is in progress.";

                // read from stream and handle by chunk
                foreach (AggregateRootEvent evnt in this.eventStreamProvider.GetEventStream())
                {
                    this.ProcessStatistics(evnt);
                    chunk.Add(evnt);
                    counter++;

                    if (counter == currentChunkSize)
                    {
                        if (this.status.Progress < 90)
                        {
                            this.status.Progress = this.status.Progress + 5;
                        }

                        if (!this.streamCollector.Collect(chunk))
                        {
                            this.status.Result = false;
                            this.status.ErrorMessage = "Target refused stream.";
                            this.status.Progress = 98;
                            this.status.IsWorking = false;
                            return;

                            // throw new Exception("Target refused stream");
                        }

                        chunk = new List<AggregateRootEvent>();
                        counter = 0;
                    }
                }

                // process partial completed chunk
                if (counter > 0)
                {
                    if (this.status.Progress < 90)
                    {
                        this.status.Progress = this.status.Progress + 5;
                    }

                    if (!this.streamCollector.Collect(chunk))
                    {
                        this.status.Result = false;
                        this.status.ErrorMessage = "Target refused stream.";
                        this.status.Progress = 98;
                        this.status.IsWorking = false;
                        return;

                        // throw new Exception("Target refused stream");
                    }
                }

                // write stat for sync
                if (this.streamCollector.SupportSyncStat)
                {
                    this.Invoker.Execute(new PushStatisticsCommand(this.ProcessGuid, this.streamCollector.GetStat()));
                }

                this.status.Progress = 95;
                this.status.CurrentStageDescription = "Finishing current stage.";

                // notify collector about finishing
                this.streamCollector.Finish();

                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed, "Ok"));

                this.status.CurrentStageDescription = "Stage finished.";
                this.status.Result = true;
                this.status.Progress = 98;
            }
            catch (Exception e)
            {
                // Logger logger = LogManager.GetCurrentClassLogger();
                // logger.Fatal("Import error", e);
                //this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error, e.Message));

                this.status.ErrorMessage = "Error occured during synchronization. [" + this.syncMessage + "]\r\n" + e.Message;
                this.status.Progress = 98;
                this.status.IsWorking = false;
                throw;

                // return ErrorCodes.Fail;
            }
            finally
            {
                this.status.IsWorking = false;
            }
        }

        #endregion
    }
}