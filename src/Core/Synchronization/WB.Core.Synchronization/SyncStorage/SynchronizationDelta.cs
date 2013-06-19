using System;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDelta : IView
    {
        public SynchronizationDelta(Guid id, string content, long sequence, Guid userId)
        {
            Id = id;
            Content = content;
            Sequence = sequence;
            UserId = userId;
        }

        public Guid Id { get; private set; }
        public string Content { get; private set; }
        public long Sequence { get; private set; }
        public Guid UserId { get; private set; }
    }
}