// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcesorStub.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClientTests.Stubs
{
    using System.Collections.Generic;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;
    using Main.Synchronization.SycProcessRepository;

    using Ncqrs.Eventing;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessorStub : ISyncProcessor
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessorStub"/> class.
        /// </summary>
        public SyncProcessorStub()
        {
        }

        #endregion
        

        #region Public Methods and Operators

        public void PostProcess()
        {
            
        }

        /// <summary>
        /// The commit.
        /// </summary>
        public void Commit()
        {
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public void Merge(IEnumerable<AggregateRootEvent> stream)
        {
        }

        #endregion
    }
}