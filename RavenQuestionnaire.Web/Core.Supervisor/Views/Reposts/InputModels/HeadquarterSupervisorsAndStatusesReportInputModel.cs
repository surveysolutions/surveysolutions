using System;

namespace Core.Supervisor.Views.Reposts.InputModels
{
    public class HeadquarterSupervisorsAndStatusesReportInputModel : ListViewModelBase
    {
        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }
    }
}