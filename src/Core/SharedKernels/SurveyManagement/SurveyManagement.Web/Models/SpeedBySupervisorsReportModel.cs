using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class SpeedBySupervisorsReportModel : IGridRequest<SpeedBySupervisorsReportInputModel>
    {
        public PagerData Pager { get; set; }

        public SpeedBySupervisorsReportInputModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}
