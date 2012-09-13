// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProgressFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The sync progress factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Synchronization
{
    using Main.Core.Documents;

    using RavenQuestionnaire.Core.ViewSnapshot;

    /// <summary>
    /// The sync progress factory.
    /// </summary>
    public class SyncProgressFactory : IViewFactory<SyncProgressInputModel, SyncProgressView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IViewSnapshot store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProgressFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public SyncProgressFactory(IViewSnapshot store)
        {
            this.store = store;
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
        /// The RavenQuestionnaire.Core.Views.Synchronization.SyncProgressView.
        /// </returns>
        public SyncProgressView Load(SyncProgressInputModel input)
        {
            var process = this.store.GetByGuid<SyncProcessDocument>(input.ProcessKey);
            if (process == null)
            {
                return null;
            }

            return new SyncProgressView(process);
        }

        #endregion
    }
}