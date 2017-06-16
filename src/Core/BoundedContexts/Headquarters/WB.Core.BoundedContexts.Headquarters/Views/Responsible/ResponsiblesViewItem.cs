using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Responsible
{
    public class ResponsiblesViewItem
    {
        public Guid SupervisorId { get; set; }
        public Guid? InterviewerId { get; set; }
        public string UserName { get; set; }

        public Guid ResponsibleId => InterviewerId ?? SupervisorId;
    }
}