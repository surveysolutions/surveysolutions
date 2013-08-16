using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class SupervisorSurveysAndStatusesReportInputModel : ListViewModelBase
    {
        public SupervisorSurveysAndStatusesReportInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }

        public Guid? UserId { get; set; }
    }
}
