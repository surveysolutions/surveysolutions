namespace Main.Core.Events.Sync
{
    using System;

    [Serializable]
    public class NewClientDeviceCreated
    {
        public Guid Id { set; get; }
        public DateTime CreationDate { set; get; }
        public string DeviceId { set; get; }
        public Guid ClientInstanceKey { set; get; }

        //public string DeviceType { set; get; }
    }
}
