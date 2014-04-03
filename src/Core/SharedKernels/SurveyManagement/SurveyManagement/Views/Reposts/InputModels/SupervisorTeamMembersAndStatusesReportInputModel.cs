using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
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
