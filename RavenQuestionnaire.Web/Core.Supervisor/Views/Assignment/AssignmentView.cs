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

    using Core.Supervisor.Views.Summary;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
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
        public AssignmentView()
        {
        }

        public AssignmentView(int page, int pageSize, int totalCount)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        public void SetItems(IEnumerable<CompleteQuestionnaireBrowseItem> items)
        {
            this.Headers = new Dictionary<Guid, string>();
            this.Items = new List<AssignmentViewItem>();
            if (items == null)
            {
                return;
            }
            if (this.Template!=null)
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
            else
            {
                this.Headers.Add(Guid.Empty, "Featured Questions");
                foreach (CompleteQuestionnaireBrowseItem it in items)
                    this.Items.Add(new AssignmentViewItem(it));
            }
        }

        public TemplateLight Template { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

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
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        #endregion

    }
}