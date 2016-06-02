namespace WB.Core.BoundedContexts.Headquarters.Views.Interviews
{
    public class TeamInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanBeReassigned { get; set; }
        public bool CanApproveOrReject { get; set; }
        public bool ReceivedByInterviewer { get; set; }
    }
}
