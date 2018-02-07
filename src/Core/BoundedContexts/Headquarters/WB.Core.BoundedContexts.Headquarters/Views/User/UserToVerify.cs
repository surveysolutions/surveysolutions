namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserToVerify
    {
        public string UserName { get; set; }
        public bool IsLocked { get; set; }
        public bool IsSupervisorOrInterviewer => IsInterviewer || IsSupervisor;
        public bool IsSupervisor { get; set; }
        public bool IsInterviewer { get; set; }
    }
}