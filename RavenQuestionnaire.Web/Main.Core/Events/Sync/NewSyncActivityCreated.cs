using System;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class NewSyncActivityCreated
    {
        public Guid Id { set; get; }
        public Guid DeviceId { set; get; }
        public DateTime CreationDate { set; get; }
    }
}
