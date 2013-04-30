// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridDataRequestModel.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the GridDataRequestModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System;
    using System.Collections.Generic;
   

    using Main.Core.Entities;

    /// <summary>
    /// Define GridDataRequest model for sorting
    /// </summary>
    public class GridDataRequestModel
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets SortOrder.
        /// </summary>
        public List<OrderRequestItem> SortOrder { get; set; }

        /// <summary>
        /// Gets or sets Pager.
        /// </summary>
        public PagerData Pager { get; set; }
        
        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets StatusId.
        /// </summary>
        public Guid StatusId { get; set; }
    }

    /// <summary>
    /// Define Pager data for paging
    /// </summary>
    public class PagerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagerData"/> class.
        /// </summary>
        public PagerData()
        {
            this.Page = 1;
            this.PageSize = 20;
        }
        /// <summary>
        /// Gets or sets Page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets PageSize.
        /// </summary>
        public int PageSize { get; set; }
    }
}