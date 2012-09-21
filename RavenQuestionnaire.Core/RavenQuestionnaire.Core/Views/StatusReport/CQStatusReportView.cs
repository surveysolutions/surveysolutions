// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQStatusReportView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq status report view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;

    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

    /// <summary>
    /// The cq status report view.
    /// </summary>
    public class CQStatusReportView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQStatusReportView"/> class.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="statisticDocuments">
        /// The statistic documents.
        /// </param>
        public CQStatusReportView(StatusItem status, IEnumerable<CompleteQuestionnaireBrowseItem> statisticDocuments)
        {
            this.StatusName = status.Title;

            this.Items = new List<CQStatusReportItemView>();
            foreach (CompleteQuestionnaireBrowseItem statisticDocument in statisticDocuments)
            {
                var view = new CQStatusReportItemView
                    {
                        AssignToUser = statisticDocument.Responsible, 
                        LastChangeDate = statisticDocument.LastEntryDate, 
                        LastSyncDate = DateTime.Now, 
                        Description =
                            string.Join(
                                ", ", 
                                statisticDocument.FeaturedQuestions.Select(f => f.Title + ": " + f.Answer.ToString()))
                    };
                this.Items.Add(view);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CQStatusReportItemView> Items { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the status name.
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}