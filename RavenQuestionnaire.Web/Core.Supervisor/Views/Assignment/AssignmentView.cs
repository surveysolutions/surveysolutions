// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignmentView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey group view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Assignment
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The survey group view.
    /// </summary>
    public class AssignmentView
    {
        #region Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string _order = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentView"/> class.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="surveyTitle">
        /// The survey title.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        public AssignmentView(int page, int pageSize, string surveyTitle, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items, Guid templateId)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.TemplateId = templateId;
            this.Items = new List<AssignmentViewItem>();
            this.Headers = new Dictionary<Guid, string>();
            if (items != null)
            {
                foreach (var question in
                    items.SelectMany(
                        completeQuestionnaireBrowseItem => completeQuestionnaireBrowseItem.FeaturedQuestions).Where(
                            question => !this.Headers.ContainsKey(question.PublicKey)))
                {
                    this.Headers.Add(question.PublicKey, question.Title);
                }
                foreach (CompleteQuestionnaireBrowseItem it in items) 
                    this.Items.Add(new AssignmentViewItem(it, this.Headers));
            }
            this.SurveyTitle = surveyTitle;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public Dictionary<Guid, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<AssignmentViewItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return this._order;
            }

            set
            {
                this._order = value;
            }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the survey title.
        /// </summary>
        public string SurveyTitle { get; private set; }

        /// <summary>
        /// Gets the template id.
        /// </summary>
        public Guid TemplateId { get; private set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}