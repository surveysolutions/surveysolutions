using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus
{
    public class CommentedStatusHistoryView
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }

        public InterviewStatus Status { get; set; }

        public string StatusHumanized { get; set; }

        public string Responsible { get; set; }
        public string ResponsibleRole { get; set; }

        public string Assignee { get; set; }
        public string AssigneeRole { get; set; }

    }
}
