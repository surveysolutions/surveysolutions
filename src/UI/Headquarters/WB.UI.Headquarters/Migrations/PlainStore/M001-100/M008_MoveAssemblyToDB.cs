using FluentMigrator;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(8)]
    public class M008_MoveAssemblyToDB : Migration
    {
        private string assemblyinfosTableName = "assemblyinfos";

        public override void Up()
        {
            var settings = ServiceLocator.Current.GetInstance<LegacyAssemblySettings>();
            var fileSystemAccessor = ServiceLocator.Current.GetInstance<IFileSystemAccessor>();

            var pathToSearch = fileSystemAccessor.CombinePath(settings.FolderPath, settings.AssembliesDirectoryName);

            var assemblyFiles = fileSystemAccessor.GetFilesInDirectory(pathToSearch, "*.dll");

            if (assemblyFiles.Length > 0)
            {
                Execute.WithConnection((con, trans) =>
                {
                    foreach (var assemblyFile in assemblyFiles)
                    {
                        var fileCreated = fileSystemAccessor.GetCreationTime(assemblyFile);
                        var fileName = fileSystemAccessor.GetFileName(assemblyFile);
                        var fileContent = fileSystemAccessor.ReadAllBytes(assemblyFile);


                        var insertCommand = con.CreateCommand();

                        insertCommand.CommandText =
                            $"INSERT INTO plainstore.\"{assemblyinfosTableName}\" (id, creationdate, content) VALUES (:id, :creationdate, :content) ON CONFLICT (id) DO NOTHING;";


                        var idParam = insertCommand.CreateParameter();
                        idParam.ParameterName = "id";
                        idParam.Value = fileName;

                        insertCommand.Parameters.Add(idParam);

                        var creationdateParam = insertCommand.CreateParameter();
                        creationdateParam.ParameterName = "creationdate";
                        creationdateParam.Value = fileCreated;

                        insertCommand.Parameters.Add(creationdateParam);

                        var contentParam = insertCommand.CreateParameter();
                        contentParam.ParameterName = "content";
                        contentParam.Value = fileContent;

                        insertCommand.Parameters.Add(contentParam);

                        insertCommand.ExecuteNonQuery();
                    }
                });
            }
        }

        public override void Down()
        {
            
        }
    }
}