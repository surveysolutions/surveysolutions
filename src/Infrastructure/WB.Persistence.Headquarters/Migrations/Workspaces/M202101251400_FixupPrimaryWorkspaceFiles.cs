using System.IO;
using FluentMigrator;
using Microsoft.Extensions.Configuration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2021_01_25_14_00)]
    public class M202101251400_FixupPrimaryWorkspaceFiles : ForwardOnlyMigration
    {
        private readonly IConfiguration configuration;

        public M202101251400_FixupPrimaryWorkspaceFiles(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override void Up()
        {
            var c = configuration.GetSection("FileStorage").Get<FileStorageConfig>();
            c.AppData = c.AppData.Replace("~", Directory.GetCurrentDirectory());
            c.TempData = c.TempData.Replace("~", Directory.GetCurrentDirectory());

            if (c.GetStorageProviderType() == StorageProviderType.FileSystem)
            {
                var primaryFolder = Path.Combine(c.AppData, "primary");

                if (Directory.Exists(primaryFolder))
                {
                    var targetFolder = c.AppData;

                    foreach (var entry in Directory.EnumerateFiles(primaryFolder, "*.*", SearchOption.AllDirectories))
                    {
                        var target = entry.Replace(primaryFolder, targetFolder);
                        var directory = Path.GetDirectoryName(target);

                        if (directory != null)
                        {
                            Directory.CreateDirectory(directory);
                            File.Move(entry, target, true);
                        }
                    }

                    Directory.Delete(primaryFolder);
                }
            }
        }
    }
}
