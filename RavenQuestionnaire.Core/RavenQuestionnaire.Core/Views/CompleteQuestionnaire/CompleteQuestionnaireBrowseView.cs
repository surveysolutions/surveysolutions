// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System.Collections.Generic;

    /// <summary>
    /// The complete questionnaire browse view.
    /// </summary>
    public class CompleteQuestionnaireBrowseView
    {
        #region Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string _order = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseView"/> class.
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
        public CompleteQuestionnaireBrowseView(
            int page, int pageSize, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<CompleteQuestionnaireBrowseItem> Items { get; private set; }

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
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}