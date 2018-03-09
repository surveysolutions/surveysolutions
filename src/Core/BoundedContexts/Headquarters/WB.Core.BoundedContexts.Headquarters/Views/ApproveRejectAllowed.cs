namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class ApproveRejectAllowed
    {
        public bool SupervisorApproveAllowed { get; set; }

        public bool HqOrAdminApproveAllowed { get; set; }

        public bool SupervisorRejectAllowed { get; set; }

        public bool HqOrAdminRejectAllowed { get; set; }

        public bool HqOrAdminUnapproveAllowed { get; set; }

        public bool InterviewerShouldbeSelected { get; set; }

        public string InterviewersListUrl { get; set; }
    }
}
