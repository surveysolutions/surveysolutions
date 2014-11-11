using System;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class SyncActivityUpdated
    {
        public Guid Id { set; get; }

        public DateTime ChangeDate { set; get; }
    }
}