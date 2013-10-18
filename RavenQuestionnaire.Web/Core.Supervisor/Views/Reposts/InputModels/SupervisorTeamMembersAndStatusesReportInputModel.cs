using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.InputModels
{
    public class SupervisorTeamMembersAndStatusesReportInputModel : ListViewModelBase
    {
        public SupervisorTeamMembersAndStatusesReportInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }

        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }
    }
}
