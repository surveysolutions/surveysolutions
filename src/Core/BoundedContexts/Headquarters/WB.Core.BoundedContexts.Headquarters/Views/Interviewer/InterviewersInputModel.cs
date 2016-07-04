using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviewer
{
    public class InterviewersInputModel : ListViewModelBase
    {
        public string SearchBy { get; set; }
        public Guid ViewerId {get; set; }
        public string SupervisorName { get; set; }
        public bool Archived { get; set; }
        public bool? ConnectedToDevice { get; set; }
    }
}