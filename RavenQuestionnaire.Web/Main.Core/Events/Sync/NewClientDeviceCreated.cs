namespace Main.Core.Events.Sync
{
    using System;

    [Serializable]
    public class NewClientDeviceCreated
    {
        public Guid Id;
        public DateTime CreationDate;
        public string DeviceId;
        public Guid ClientInstanceKey;

        public string DeviceType;
    }
}
