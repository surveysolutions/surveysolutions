using System;
using System.Collections.Generic;
using System.Reflection;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(202103101642)]
    public class M202103101642_MoveSqliteTablesInWorkspaceFolder : IMigration
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPrincipal principal;
        private readonly SqliteSettings settings;

        public M202103101642_MoveSqliteTablesInWorkspaceFolder(IFileSystemAccessor fileSystemAccessor,
            IPrincipal principal,
            SqliteSettings settings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.principal = principal;
            this.settings = settings;
        }

        public void Up()
        {
            var workspace = principal.CurrentUserIdentity?.Workspace ?? "primary";
            if (string.IsNullOrWhiteSpace(workspace))
                return;

            var workspaceFolder = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, workspace);
            if (!fileSystemAccessor.IsDirectoryExists(workspaceFolder))
                fileSystemAccessor.CreateDirectory(workspaceFolder);

            var files = fileSystemAccessor.GetFilesInDirectory(settings.PathToDatabaseDirectory, "*-data.sqlite3");

            var excludeFiles = new List<string>()
            {
                "Migration",
                "InterviewerIdentity",
                "SupervisorIdentity",
                "TesterUserIdentity",
                "WorkspaceView",
                "AuditLogSettingsView",
                "AuditLogRecordView",
                "AuditLogEntityView",
                "ApplicationSettingsView",
                "EnumeratorSettingsView",
            };
            
            foreach (var file in files)
            {
                var fileName = fileSystemAccessor.GetFileName(file);
                var entityTypeName = fileName.Replace("-data.sqlite3", "");
                if (excludeFiles.Contains(entityTypeName))
                    continue;

                // var entityType = Type.GetType(entityTypeName);
                // var workspacesAttribute = entityType.GetCustomAttribute<WorkspacesAttribute>();
                // if (workspacesAttribute != null)
                //     continue;

                var newFilePath = fileSystemAccessor.CombinePath(workspaceFolder, fileName);
                fileSystemAccessor.MoveFile(file, newFilePath);
            }
        }
    }
}