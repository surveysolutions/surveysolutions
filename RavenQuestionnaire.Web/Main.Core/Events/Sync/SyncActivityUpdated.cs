using System;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class SyncActivityUpdated
    {
        public Guid Id;
        public DateTime ChangeDate;
    }
}