using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog
{
    public class AuditLogSettingsView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public int LastSyncedEntityId { get; set; }
    }
}
