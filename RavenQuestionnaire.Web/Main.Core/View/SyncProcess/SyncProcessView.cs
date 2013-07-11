namespace Main.Core.View.SyncProcess
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The user view.
    /// </summary>
    public class SyncProcessView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessView"/> class. 
        /// </summary>
        /// <param name="process">
        /// Sync Process Statistics Document
        /// </param>
        public SyncProcessView(SyncProcessStatisticsDocument process)
        {
            this.Messages = new List<SyncStatisticInfo>();
            if (process == null)
                return;
            var statistics = new Dictionary<Guid, SyncStatisticInfo>();
            foreach (var statistic in process.Statistics)
            {
                if (statistic.User == null)
                {
                    continue;
                }

                if (!statistics.ContainsKey(statistic.User.Id))
                {
                    statistics.Add(statistic.User.Id, new SyncStatisticInfo(statistic.User.Name, 0, 0, 0, false));
                }

                switch (statistic.Type)
                {
                    case SynchronizationStatisticType.NewSurvey:
                        statistics[statistic.User.Id].NewAssignments++;
                        break;
                    case SynchronizationStatisticType.NewUser:
                        statistics[statistic.User.Id].IsNew = true;
                        break;
                    case SynchronizationStatisticType.AssignmentChanged:
                        statistics[statistic.User.Id].NewAssignments++;
                        break;
                    case SynchronizationStatisticType.NewAssignment:
                        statistics[statistic.User.Id].NewAssignments++;
                        break;
                    case SynchronizationStatisticType.StatusChanged:
                        if (statistic.Status.PublicId == SurveyStatus.Redo.PublicId)
                        {
                            statistics[statistic.User.Id].Rejected++;
                        }
                        else if (statistic.Status.PublicId == SurveyStatus.Approve.PublicId)
                        {
                            statistics[statistic.User.Id].Approved++;
                        }

                        break;
                }
            }

            this.Messages.AddRange(statistics.Values);
            this.PublicKey = process.PublicKey;
            this.SyncType = process.SyncType;
            this.IsEnded = process.IsEnded;
            this.CreationDate = process.CreationDate;
            this.EndDate = process.EndDate;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets messages.
        /// </summary>
        public List<SyncStatisticInfo> Messages { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsEnded.
        /// </summary>
        public bool IsEnded { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public SynchronizationType SyncType { get; set; }

        #endregion
    }
}