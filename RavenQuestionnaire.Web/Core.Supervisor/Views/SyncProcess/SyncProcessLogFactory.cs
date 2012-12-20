﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessLogFactory.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.SyncProcess
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessLogFactory : IViewFactory<SyncProcessLogInputModel, SyncProcessLogView>
    {
        #region Constants and Fields

        /// <summary>
        /// The docs.
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessStatisticsDocument> docs;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessLogFactory"/> class.
        /// </summary>
        /// <param name="docs">
        /// The docs.
        /// </param>
        public SyncProcessLogFactory(IDenormalizerStorage<SyncProcessStatisticsDocument> docs)
        {
            this.docs = docs;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// </returns>
        public SyncProcessLogView Load(SyncProcessLogInputModel input)
        {
            IQueryable<SyncProcessStatisticsDocument> processes =
                this.docs.Query().OrderByDescending(p => p.CreationDate);

            return new SyncProcessLogView(processes);
        }

        #endregion
    }
}