using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.SystemLog
{
    public class SystemLogEntry
    {
        public virtual int Id { get; set; }
        public virtual Guid? UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual LogEntryType Type { get; set; }
        public virtual DateTime LogDate { get; set; }
        public virtual string Log { get; set; }
    }


}
