using System;
using System.Collections.Generic;
using System.IO;
using FluentMigrator;
using WB.Infrastructure.Native;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(8)]
    public class M008_MoveAssemblyToDB : Migration
    {
        private readonly LegacyAssemblySettings settings;
        private string assemblyinfosTableName = "assemblyinfos";

        public M008_MoveAssemblyToDB(LegacyAssemblySettings settings)
        {
            this.settings = settings;
        }

        public override void Up()
        {
            //var settings = ServiceLocator.Current.GetInstance<LegacyAssemblySettings>();

            if (settings?.AssembliesDirectoryName == null) 
                return;

            var pathToSearch = Path.Combine(settings.FolderPath, settings.AssembliesDirectoryName);

            var assemblyFiles = Directory.Exists(pathToSearch) ? Directory.GetFiles(pathToSearch,  "*.dll") : new string[0];

            if (assemblyFiles.Length > 0)
            {
                Execute.WithConnection((con, trans) =>
                {
                    foreach (var assemblyFile in assemblyFiles)
                    {
                        var fileCreated = File.Exists(assemblyFile) ? new FileInfo(assemblyFile).CreationTime : DateTime.MinValue;
                        var fileName = Path.GetFileName(assemblyFile);
                        var fileContent = File.ReadAllBytes(assemblyFile);

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
            if (settings?.AssembliesDirectoryName == null) 
                return;

            var pathToSave = Path.Combine(settings.FolderPath, settings.AssembliesDirectoryName);

            List<string> assemblies = new List<string>();

            Execute.WithConnection((con, trans) =>
            {
                var dbCommand = con.CreateCommand();
                dbCommand.CommandText = $"select id from plainstore.\"{assemblyinfosTableName}\"";
                using (var reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assemblies.Add(reader[0] as string);
                    }
                }

                foreach (var assemblyId in assemblies)
                {
                    var shareDataCommand = con.CreateCommand();
                    shareDataCommand.CommandText = @"select a.id as assemblyName, a.content as content from plainstore.assemblyinfos a where id = :assemblyId)";

                    var dbDataParameter = shareDataCommand.CreateParameter();
                    dbDataParameter.ParameterName = "assemblyId";
                    dbDataParameter.Value = assemblyId;
                    shareDataCommand.Parameters.Add(dbDataParameter);

                    using (var reader = shareDataCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var assemblyName = reader["assemblyName"] as string;
                            var content = reader["content"] as byte[];

                            File.WriteAllBytes(Path.Combine(pathToSave, assemblyName), content);
                        }
                    }
                }
                
            });

            Execute.WithConnection((con, trans) =>
            {
                var dbCommand = con.CreateCommand();
                dbCommand.CommandText = $"delete from plainstore.\"{assemblyinfosTableName}\"";

                dbCommand.ExecuteNonQuery();
            });
        }
    }
}
