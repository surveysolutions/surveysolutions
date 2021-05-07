using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog
{
    public class SystemLogItem
    {
        public virtual int Id { get; set; }
        public virtual Guid? UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Type { get; set; }
        public virtual DateTime LogDate { get; set; }
        public virtual string Log { get; set; }
    }
}
