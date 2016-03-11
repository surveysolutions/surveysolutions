using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Views.BrokenInterviewPackages
{
    public class BrokenInterviewPackageFilter
    {
        public DateTime? FromProcessingDateTime { get; set; }
        public DateTime? ToProcessingDateTime { get; set; }
        public string ExceptionType { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}