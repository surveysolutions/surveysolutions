using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DocumentListViewModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }

        public Guid? TemplateId { get; set; }
        public long? TemplateVersion { get; set; }
        public string ResponsibleName { get; set; }

        public int? AssignmentId { get; set; }

        public InterviewStatus? Status { get; set; }
        public string SearchBy { get; set; }

        public DateTime? UnactiveDateStart { get; set; }
        public DateTime? UnactiveDateEnd { get; set; }

        public Guid? TeamId { get; set; }
    }
}
