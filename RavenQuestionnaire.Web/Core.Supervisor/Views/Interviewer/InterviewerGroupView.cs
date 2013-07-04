using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.View.CompleteQuestionnaire;

namespace Core.Supervisor.Views.Interviewer
{
    using Core.Supervisor.Views.Survey;

    /// <summary>
    /// The interviewer group view.
    /// </summary>
    public class InterviewerGroupView
    {
        #region Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string order = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerGroupView"/> class.
        /// </summary>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="order">
        /// The order.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        public InterviewerGroupView(
            Guid templateId, 
            string title, 
            List<CompleteQuestionnaireBrowseItem> items, 
            string order, 
            int page, 
            int pageSize, 
            int totalCount)
        {
            this.TemplateId = templateId;
            this.Title = title;
            this.Items = items;
            this.Order = order;
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            var helper = new Dictionary<Guid, string>();
            foreach (var question in
                items.SelectMany(
                    completeQuestionnaireBrowseItem =>
                    completeQuestionnaireBrowseItem.FeaturedQuestions.Where(t => !string.IsNullOrEmpty(t.Title)))
                    .Where(question => !helper.ContainsKey(question.PublicKey)))
            {
                helper.Add(question.PublicKey, question.Title);
            }

            this.HeaderFeaturedQuestions = helper;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the header featured questions.
        /// </summary>
        public Dictionary<Guid, string> HeaderFeaturedQuestions { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<CompleteQuestionnaireBrowseItem> Items { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return this.order;
            }

            set
            {
                this.order = value;
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
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        public int TotalCount { get; private set; }

        #endregion
    }
}