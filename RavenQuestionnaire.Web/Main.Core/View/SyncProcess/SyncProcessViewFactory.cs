// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Main.Core.View.SyncProcess
{
    using System;

    using Main.Core.Documents;
    using Main.Core.View.User;

    /// <summary>
    /// The user view factory.
    /// </summary>
    public class SyncProcessViewFactory : IViewFactory<SyncProcessInputModel, SyncProcessView>
    {
        #region Constants and Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessStatisticsDocument> docs;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessViewFactory"/> class.
        /// </summary>
        /// <param name="docs">
        /// The docs.
        /// </param>
        public SyncProcessViewFactory(IDenormalizerStorage<SyncProcessStatisticsDocument> docs)
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
        /// The RavenQuestionnaire.Core.Views.User.UserView.
        /// </returns>
        public SyncProcessView Load(SyncProcessInputModel input)
        {
            SyncProcessStatisticsDocument process = this.docs.GetById(input.SyncKey);

            if (process == null || !process.IsEnded)
            {
                return new SyncProcessView(null);
            }

            return new SyncProcessView(process);
        }

        #endregion
    }
}