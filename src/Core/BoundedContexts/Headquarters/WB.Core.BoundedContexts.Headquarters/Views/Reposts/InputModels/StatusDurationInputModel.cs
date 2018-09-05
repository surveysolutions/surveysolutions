using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels
{
    public class StatusDurationInputModel : ListViewModelBase
    {
        public Guid? TemplateId { get; set; }
        public long? TemplateVersion { get; set; }
        public int MinutesOffsetToUtc { get; set; }
        public Guid? SupervisorId { get; set; }
    }
}
