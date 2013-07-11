using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.View.CompleteQuestionnaire;

namespace Main.Core.View.StatusReport
{
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
        public CQStatusReportView(IEnumerable<CompleteQuestionnaireBrowseItem> statisticDocuments)
        {

            this.Items = new List<CQStatusReportItemView>();
            foreach (CompleteQuestionnaireBrowseItem statisticDocument in statisticDocuments)
            {
                var view = new CQStatusReportItemView(statisticDocument.CompleteQuestionnaireId,
                                                      statisticDocument.Responsible,

                                                      string.Join(
                                                          ", ",
                                                          statisticDocument.FeaturedQuestions.Select(
                                                              f => f.Title + ": " + f.Answer.ToString())),
                                                      statisticDocument.LastEntryDate,
                                                      DateTime.Now
                    );
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
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}