namespace WB.Core.BoundedContexts.Headquarters.Views.Interviews
{
    public class TeamInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanBeReassigned { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool ReceivedByInterviewer { get; set; }
        public bool IsNeedInterviewerAssign { get; set; }
    }
}
