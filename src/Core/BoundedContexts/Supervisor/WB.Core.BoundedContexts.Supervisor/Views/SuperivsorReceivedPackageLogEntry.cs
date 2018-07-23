using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class SuperivsorReceivedPackageLogEntry : IPlainStorageEntity<int>
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public Guid FirstEventId { get; set; }
        public Guid LastEventId { get; set; }
        public DateTime FirstEventTimestamp { get; set; }
        public DateTime LastEventTimestamp { get; set; }
    }
}
