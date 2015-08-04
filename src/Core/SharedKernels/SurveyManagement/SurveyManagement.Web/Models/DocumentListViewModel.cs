using System.Collections.Generic;
using Main.Core.Entities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DocumentListViewModel : IGridRequest<DocumentRequestModel>
    {
        public PagerData Pager { get; set; }

        public DocumentRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}