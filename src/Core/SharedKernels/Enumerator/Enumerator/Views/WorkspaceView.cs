using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    [Workspaces]
    public class WorkspaceView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Disabled { get; set; }
    }
}