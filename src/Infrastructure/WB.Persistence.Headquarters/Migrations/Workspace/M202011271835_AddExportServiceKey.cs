using System;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2020_11_27_18_35)]
    public class M202011271835_AddExportServiceKey : ForwardOnlyMigration
    {
        public override void Up()
        {
            var serviceKey = Guid.NewGuid();
            var serializeObject = JsonConvert.SerializeObject(new
            {
                Key = serviceKey.ToString()
            });

            Execute.Sql($@"INSERT INTO appsettings (id, value) VALUES ('ExportService.ApiKey', '{serializeObject}')
                ON CONFLICT (id) DO NOTHING;");
        }
    }
}
