using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class QuantityByInterviewersReportModel : IGridRequest<QuantityByInterviewersReportInputModel>
    {
        public PagerData Pager { get; set; }

        public QuantityByInterviewersReportInputModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}
