using System;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.Sync
{
    [Serializable]
    public class ClientDeviceLastSyncItemUpdated
    {
        public Guid Id { set; get; }

        public DateTime ChangeDate { set; get; }

        public long LastSyncItemSequence { set; get; }

    }
}
