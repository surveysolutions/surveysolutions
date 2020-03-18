using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog
{
    public class SynchronizationLogItem
    {
        public virtual int Id { get; set; }
        public virtual Guid InterviewerId { get; set; }
        public virtual Guid? InterviewId { get; set; }
        public virtual string InterviewerName { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual DateTime LogDate { get; set; }
        public virtual SynchronizationLogType Type { get; set; }
        public virtual string Log { get; set; }
        public virtual string ActionExceptionType { get; set; }
        public virtual string ActionExceptionMessage { get; set; }
    }
}
