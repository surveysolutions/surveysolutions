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

    using Main.Core.Events;
    using Main.Synchronization.SyncSreamProvider;
    using Main.Synchronization.SyncStreamCollector;

    /// <summary>
    /// The sync manager.
    /// </summary>
    public class SyncManager : ISyncManager
    {
        #region Fields

        /// <summary>
        /// The chunk size. Memory usage limitation.
        /// </summary>
        private const int ChunkSize = 1024;

        /// <summary>
        /// The event stream provider.
        /// </summary>
        private readonly ISyncEventStreamProvider eventStreamProvider;

        /// <summary>
        /// The stream collector.
        /// </summary>
        private readonly ISyncStreamCollector streamCollector;

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
        /// <param name="settings">
        /// The settings.
        /// </param>
        public SyncManager(
            ISyncEventStreamProvider provider,
            ISyncStreamCollector collector,
            Guid syncProcess,
            SyncManagerSettings settings)
        {
            this.eventStreamProvider = provider;
            this.managerSettings = settings;
            this.ProcessGuid = syncProcess;
            this.streamCollector = collector;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is working.
        /// </summary>
        public bool IsWorking { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the process guid.
        /// </summary>
        protected Guid ProcessGuid
        {
            get;
            private set;
        }

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
            Pull();

            this.EndTime = DateTime.UtcNow;
        }

        private void Pull()
        {
            throw new NotImplementedException();
        }

        public DateTime StartTime
        {
            get;
            private set;
        }

        private void ProcessStatistics(AggregateRootEvent evnt)
        {
            // cteate statistics processor
        }

        /// <summary>
        /// The start push.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartPush()
        {
            this.StartTime = DateTime.UtcNow;
            
            Push();

            this.EndTime = DateTime.UtcNow;
        }

        public DateTime EndTime
        {
            get;

            private set;

        }

        private void Push()
        {
            this.streamCollector.PrepareToCollect();

            int currentChunkSize = Math.Min(this.streamCollector.MaxChunkSize, ChunkSize);
            int counter = 0;

            List<AggregateRootEvent> chunk = new List<AggregateRootEvent>();

            try
            {
                foreach (var evnt in this.eventStreamProvider.GetEventStream())
                {
                    if (counter == currentChunkSize)
                    {
                        this.streamCollector.Collect(chunk);
                        chunk = new List<AggregateRootEvent>();
                        counter = 0;
                    }

                    this.ProcessStatistics(evnt);
                    chunk.Add(evnt);
                    counter++;
                }

                this.streamCollector.Collect(chunk);
            }
            catch (Exception e)
            {
            }
            finally
            {
            }

            this.streamCollector.Finish();
        }

        /// <summary>
        /// The start synchronization.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartSynchronization()
        {
            this.StartTime = DateTime.UtcNow;
            Push();

            Pull();
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
    }
}