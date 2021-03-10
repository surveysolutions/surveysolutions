using System;
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

        [Ignore]
        public string Name => Id;
        public string DisplayName { get; set; }
        public bool Disabled { get; set; }
        public Guid? SupervisorId { get; set; }
    }
}