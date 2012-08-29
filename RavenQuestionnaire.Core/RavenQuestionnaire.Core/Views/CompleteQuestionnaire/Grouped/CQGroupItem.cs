// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQGroupItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq group item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Utility;

    /// <summary>
    /// The cq group item.
    /// </summary>
    public class CQGroupItem
    {
        #region Fields

        /// <summary>
        /// The _id.
        /// </summary>
        private string _id;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQGroupItem"/> class.
        /// </summary>
        public CQGroupItem()
        {
            this.Items = new CompleteQuestionnaireBrowseItem[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CQGroupItem"/> class.
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
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        public CQGroupItem(
            int page, 
            int pageSize, 
            int totalCount, 
            /*IEnumerable<CompleteQuestionnaireStatisticDocument> items,*/ string title, 
            string id)
            : this()
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;

            // this.Items = items.Select(x => new CompleteQuestionnaireBrowseItem(x));
            this.SurveyTitle = title;
            this.SurveyId = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public IList<CompleteQuestionnaireBrowseItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the survey id.
        /// </summary>
        public string SurveyId
        {
            get
            {
                return IdUtil.ParseId(this._id);
            }

            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// Gets or sets the survey title.
        /// </summary>
        public string SurveyTitle { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        #endregion
    }
}