using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewAction
    {
        public virtual int Id { get; set; }
        public virtual InterviewExportedAction Action { get; set; }
        public virtual string Originator { get; set; }
        public virtual string Role { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual InterviewHistory History { get; set; }
    }
}