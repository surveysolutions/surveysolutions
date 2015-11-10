using System;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.Sync
{
    [Serializable]
    public class ClientDeviceLastSyncItemUpdated : ILiteEvent
    {
        public Guid Id { set; get; }

        public DateTime ChangeDate { set; get; }

        public long LastSyncItemSequence { set; get; }

    }
}
