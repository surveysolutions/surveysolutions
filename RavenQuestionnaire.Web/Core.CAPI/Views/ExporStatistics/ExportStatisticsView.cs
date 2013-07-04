namespace Core.CAPI.Views.ExporStatistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.SyncProcess;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExportStatisticsView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportStatisticsView"/> class.
        /// </summary>
        /// <param name="cqs">
        /// The cqs.
        /// </param>
        public ExportStatisticsView(IEnumerable<CompleteQuestionnaireBrowseItem> cqs)
        {
            this.Items = new List<SyncStatisticInfo>();
            var dict = new Dictionary<Guid, SyncStatisticInfo>();
            foreach (var cq in cqs)
            {
                if (dict.ContainsKey(cq.Responsible.Id))
                {
                    dict[cq.Responsible.Id].Approved++;
                }
                else
                {
                    dict.Add(cq.Responsible.Id, new SyncStatisticInfo(cq.Responsible.Name, 0, 1, 0, false));
                }
            }

            this.Items.AddRange(dict.Values);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<SyncStatisticInfo> Items { get; set; }

        #endregion
    }
}