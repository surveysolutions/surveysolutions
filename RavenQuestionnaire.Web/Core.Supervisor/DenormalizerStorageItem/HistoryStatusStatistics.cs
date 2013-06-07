// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HistoryStatusStatistics.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Core.Supervisor.DenormalizerStorageItem
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    #warning TLK: delete as no longer needed
    [Obsolete]
    public class HistoryStatusStatistics : IView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryStatusStatistics"/> class.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        public HistoryStatusStatistics(DateTime date)
        {
            this.Date = date;
            this.Data = new Dictionary<SurveyStatus, List<Guid>>();
            foreach (SurveyStatus status in SurveyStatus.GetAllStatuses())
            {
                this.Data.Add(status, new List<Guid>());
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Data.
        /// </summary>
        public Dictionary<SurveyStatus, List<Guid>> Data { get; set; }

        /// <summary>
        /// Gets or sets Date.
        /// </summary>
        public DateTime Date { get; set; }

        #endregion

        public void Remove(SurveyStatus status, Guid cqId)
        {
        }

        public void Add(SurveyStatus status, Guid cqId)
        {
        }
    }
}