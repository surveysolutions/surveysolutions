// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace WB.UI.Designer.Views.Questionnaire
{
    /// <summary>
    /// The questionnaire browse view.
    /// </summary>
    public class QuestionnaireListView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireListViewItem"/> class.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        public QuestionnaireListView(
            int page, int pageSize, int totalCount, IEnumerable<QuestionnaireListViewItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order ?? string.Empty;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<QuestionnaireListViewItem> Items { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}