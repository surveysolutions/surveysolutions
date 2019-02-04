using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class DeviceByInterviewersReportInputModel : ListViewModelBase
    {
        public string Filter { get; set; }

        public Guid? SupervisorId { get; set; }
    }
}
