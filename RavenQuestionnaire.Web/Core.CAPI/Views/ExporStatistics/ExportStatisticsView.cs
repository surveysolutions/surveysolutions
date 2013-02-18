// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportStatisticsView.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.CAPI.Views.ExporStatistics
{
    using System.Collections.Generic;

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
            this.Items = new List<CapiExportStatistics>();
            foreach (CompleteQuestionnaireBrowseItem cq in cqs)
            {
                this.Items.Add(
                    new CapiExportStatistics
                        {
                            Status = cq.Status, 
                            TemplateId = cq.TemplateId, 
                            Title = cq.QuestionnaireTitle, 
                            User = cq.Responsible, 
                            SurveyId = cq.CompleteQuestionnaireId
                        });
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public List<CapiExportStatistics> Items { get; set; }

        #endregion
    }
}