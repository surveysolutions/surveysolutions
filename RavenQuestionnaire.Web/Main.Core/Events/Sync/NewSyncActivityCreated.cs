using System;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class NewSyncActivityCreated
    {
        public Guid Id;
        public Guid DeviceId;
        public DateTime CreationDate;
    }
}
