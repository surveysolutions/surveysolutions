using System;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class TabletLog
    {
        public virtual int Id { get; set; }

        public virtual string DeviceId { get; set; }

        public virtual string UserName { get; set; }

        public virtual byte[] Content { get; set; }

        public virtual DateTime ReceiveDateUtc { get; set; }
    }
}
