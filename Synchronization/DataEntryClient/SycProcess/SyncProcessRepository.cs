// -----------------------------------------------------------------------
// <copyright file="SyncProcessRepository.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClient.SycProcess
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
        /// <summary>
        /// The users
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        /// <summary>
        /// the surveys
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// the processes
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessStatisticsDocument> processes;

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

        /// <summary>
        /// The get process
        /// </summary>
        /// <param name="synkProcessKey">
        /// The synk process key.
        /// </param>
        /// <returns>
        /// Syncronization process item
        /// </returns>
        public SyncProcess GetProcess(Guid synkProcessKey)
        {
            return new SyncProcess(this.processes.GetByGuid(synkProcessKey), this.surveys, this.users);
        }
    }
}
