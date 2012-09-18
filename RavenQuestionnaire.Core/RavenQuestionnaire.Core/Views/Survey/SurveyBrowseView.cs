// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyBrowseView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey browse view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The survey browse view.
    /// </summary>
    public class SurveyBrowseView
    {
        #region Fields

        /// <summary>
        /// The _order.
        /// </summary>
        private string _order = string.Empty;

        /// <summary>
        /// The _orders.
        /// </summary>
        private List<OrderRequestItem> _orders = new List<OrderRequestItem>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyBrowseView"/> class.
        /// </summary>
        public SurveyBrowseView()
        {
            this.Items = new List<SurveyBrowseItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyBrowseView"/> class.
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
        public SurveyBrowseView(int page, int pageSize, int totalCount, IEnumerable<SurveyBrowseItem> items)
            : this()
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Headers = new Dictionary<Guid, string>
                {
                    { Guid.NewGuid(), "Total" }, 
                    { Guid.NewGuid(), "Unassigned" }, 
                    { SurveyStatus.Initial.PublicId, SurveyStatus.Initial.Name }, 
                    { SurveyStatus.Error.PublicId, SurveyStatus.Error.Name }, 
                    { SurveyStatus.Complete.PublicId, SurveyStatus.Complete.Name }, 
                    { SurveyStatus.Approve.PublicId, SurveyStatus.Approve.Name },
                    { SurveyStatus.Redo.PublicId, SurveyStatus.Redo.Name }
                };
            foreach (SurveyBrowseItem item in items)
            {
                this.Items.Add(
                    new SurveyBrowseItem(
                        item.Id, 
                        item.Title, 
                        item.Unassigned, 
                        item.Statistic, 
                        item.Total, 
                        item.Initial, 
                        item.Error, 
                        item.Completed, 
                        item.Approve, 
                        item.Redo,
                        this.Headers));
            }
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
        public List<SurveyBrowseItem> Items { get; set; }

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
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderRequestItem> Orders
        {
            get
            {
                return this._orders;
            }

            set
            {
                this._orders = value;
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