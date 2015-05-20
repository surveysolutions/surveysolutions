using System;
using System.Collections.Generic;
using Main.Core.Entities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    /// <summary>
    /// Define GridDataRequest model for sorting
    /// </summary>
    public class GridDataRequestModel
    {
        
        public List<OrderRequestItem> SortOrder { get; set; }

        public PagerData Pager { get; set; }

        public Guid? TemplateId { get; set; }
        
        public Guid? StatusId { get; set; }

        public Guid? InterviwerId { get; set; }
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