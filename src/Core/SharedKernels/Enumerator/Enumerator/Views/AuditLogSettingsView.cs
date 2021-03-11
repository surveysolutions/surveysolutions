using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    [Workspaces]
    public class AuditLogSettingsView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public int LastSyncedEntityId { get; set; }
    }
}
