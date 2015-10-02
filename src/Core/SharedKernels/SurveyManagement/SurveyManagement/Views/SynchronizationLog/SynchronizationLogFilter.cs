using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog
{
    public class SynchronizationLogFilter
    {
        public string InterviewerName { get; set; }
        public string DeviceId { get; set; }
        public DateTime? FromDateTime { get; set; }
        public DateTime? ToDateTime { get; set; }
        public SynchronizationLogType? Type { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}