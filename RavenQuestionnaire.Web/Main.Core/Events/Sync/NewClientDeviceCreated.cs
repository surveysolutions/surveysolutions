using System;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class NewClientDeviceCreated
    {
        public Guid Id { set; get; }
        public DateTime CreationDate { set; get; }
        public string DeviceId { set; get; }
        public Guid ClientInstanceKey { set; get; }

        public Guid SupervisorKey { set; get; }

    }
}
