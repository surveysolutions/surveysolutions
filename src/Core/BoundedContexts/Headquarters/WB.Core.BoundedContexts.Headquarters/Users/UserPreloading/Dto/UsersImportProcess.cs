using System;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto
{
    public class UsersImportProcess
    {
        public virtual int Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual int SupervisorsCount { get; set; }
        public virtual int InterviewersCount { get; set; }
        public virtual string Responsible { get; set; }
        public virtual DateTime StartedDate { get; set; }
        public virtual string Workspace { get; set; }
    }
}
