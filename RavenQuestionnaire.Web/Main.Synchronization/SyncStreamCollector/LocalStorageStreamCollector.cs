// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalStorageStreamCollector.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The local storage stream collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncStreamCollector
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;
    using Main.Synchronization.SycProcessRepository;

    using Ninject;

    /// <summary>
    /// The local storage stream collector.
    /// </summary>
    public class LocalStorageStreamCollector : ISyncStreamCollector
    {
        #region Fields

        /// <summary>
        /// sync process repository
        /// </summary>
        protected readonly ISyncProcessRepository SyncProcessRepository;

        /// <summary>
        /// The process Guid.
        /// </summary>
        private readonly Guid processGuid;

        /// <summary>
        /// The processor.
        /// </summary>
        private ISyncProcessor processor;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalStorageStreamCollector"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        public LocalStorageStreamCollector(IKernel kernel, Guid processGuid)
        {
            this.SyncProcessRepository = kernel.Get<ISyncProcessRepository>();
            this.processGuid = processGuid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        public int MaxChunkSize
        {
            get
            {
                return 1024;
            }
        }

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
        public bool Collect(IEnumerable<AggregateRootEvent> chunk)
        {
            this.processor.Merge(chunk);
            return true;
        }
        
        /// <summary>
        /// The finish.
        /// </summary>
        public void Finish()
        {
            this.processor.Commit();
        }

        /// <summary>
        /// The prepare to collect.
        /// </summary>
        public void PrepareToCollect()
        {
            this.processor = this.SyncProcessRepository.GetProcessor(this.processGuid);
        }

        #endregion
    }
}