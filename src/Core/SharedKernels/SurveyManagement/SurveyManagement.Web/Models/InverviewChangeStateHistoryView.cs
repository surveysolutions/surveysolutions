using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InverviewChangeStateHistoryView
    {
        public IEnumerable<HistoryItemView> HistoryItems { get; set; }
    }
}