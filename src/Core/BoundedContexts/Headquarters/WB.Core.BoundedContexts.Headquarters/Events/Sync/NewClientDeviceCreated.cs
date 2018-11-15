using System;
using WB.Core.Infrastructure.EventBus;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.Sync
{
    [Serializable]
    public class NewClientDeviceCreated : IEvent
    {
        public Guid Id { set; get; }
        public DateTime CreationDate { set; get; }
        public string DeviceId { set; get; }
        public Guid ClientInstanceKey { set; get; }

        public Guid SupervisorKey { set; get; }

    }
}
