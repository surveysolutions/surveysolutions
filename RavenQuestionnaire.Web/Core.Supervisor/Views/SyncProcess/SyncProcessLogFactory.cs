// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessLogFactory.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views.SyncProcess
{
    using System.Collections.Generic;
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
        private readonly IQueryableReadSideRepositoryReader<SyncProcessStatisticsDocument> docs;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessLogFactory"/> class.
        /// </summary>
        /// <param name="docs">
        /// The docs.
        /// </param>
        public SyncProcessLogFactory(IQueryableReadSideRepositoryReader<SyncProcessStatisticsDocument> docs)
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
            IEnumerable<SyncProcessStatisticsDocument> processes =
                this.docs.Query(_ => _.OrderByDescending(p => p.CreationDate).ToList());

            return new SyncProcessLogView(processes);
        }

        #endregion
    }
}