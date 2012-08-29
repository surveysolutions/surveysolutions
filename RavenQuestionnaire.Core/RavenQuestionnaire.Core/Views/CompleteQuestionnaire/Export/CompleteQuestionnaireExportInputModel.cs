// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System;
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Utility;

    /// <summary>
    /// The complete questionnaire export input model.
    /// </summary>
    public class CompleteQuestionnaireExportInputModel
    {
        #region Fields

        /// <summary>
        /// The _orders.
        /// </summary>
        public List<OrderRequestItem> _orders = new List<OrderRequestItem>();

        /// <summary>
        /// The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int _pageSize = 20;

        /// <summary>
        /// The _questionnary id.
        /// </summary>
        private Guid _questionnaryId;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public string Order
        {
            get
            {
                return StringUtil.GetOrderRequestString(this._orders);
            }

            set
            {
                this._orders = StringUtil.ParseOrderRequestString(value);
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
        /// Gets or sets the page.
        /// </summary>
        public int Page
        {
            get
            {
                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the questionnary id.
        /// </summary>
        public Guid QuestionnaryId
        {
            get
            {
                return this._questionnaryId;
            }

            set
            {
                this._questionnaryId = value;
                
            }
        }

        #endregion
    }
}