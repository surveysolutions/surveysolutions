// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProgressInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The sync progress input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Synchronization
{
    using System;

    /// <summary>
    /// The sync progress input model.
    /// </summary>
    public class SyncProgressInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProgressInputModel"/> class.
        /// </summary>
        /// <param name="progressKey">
        /// The progress key.
        /// </param>
        public SyncProgressInputModel(Guid progressKey)
        {
            this.ProcessKey = progressKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the process key.
        /// </summary>
        public Guid ProcessKey { get; private set; }

        #endregion
    }
}