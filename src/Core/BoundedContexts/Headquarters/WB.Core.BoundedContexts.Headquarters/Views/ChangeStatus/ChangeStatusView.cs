using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus
{
    public class ChangeStatusView
    {
        public List<CommentedStatusHistroyView> StatusHistory { get; set; }
    }

    public class CommentedStatusHistroyView
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public string DateHumanized { get; set; }

        public InterviewStatus Status { get; set; }

        public string StatusHumanized { get; set; }

        public string Responsible { get; set; }
        public string ResponsibleRole { get; set; }

        public string Assignee { get; set; }
        public string AssigneeRole { get; set; }

    }
}
