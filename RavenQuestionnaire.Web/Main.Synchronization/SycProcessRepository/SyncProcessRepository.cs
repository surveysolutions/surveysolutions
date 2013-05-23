// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessRepository.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SycProcessRepository
{
    using System;

    using Main.Core.Documents;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessRepository : ISyncProcessRepository
    {
        #region Fields

        /// <summary>
        /// the processes
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessStatisticsDocument> processes;

        /// <summary>
        /// the surveys
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// The users
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessRepository"/> class.
        /// </summary>
        /// <param name="processes">
        /// The processes.
        /// </param>
        /// <param name="surveys">
        /// The surveys.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        public SyncProcessRepository(
            IDenormalizerStorage<SyncProcessStatisticsDocument> processes, 
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys, 
            IDenormalizerStorage<UserDocument> users)
        {
            this.processes = processes;
            this.surveys = surveys;
            this.users = users;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get processor.
        /// </summary>
        /// <param name="syncProcessKey">
        /// The sync process key.
        /// </param>
        /// <returns>
        /// The <see cref="ISyncProcessor"/>.
        /// </returns>
        public ISyncProcessor GetProcessor(Guid syncProcessKey)
        {
            return new SyncProcessor(this.processes.GetById(syncProcessKey), this.surveys, this.users);
        }

        #endregion
    }
}