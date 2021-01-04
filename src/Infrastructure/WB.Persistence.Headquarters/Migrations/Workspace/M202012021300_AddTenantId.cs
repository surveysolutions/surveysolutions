using System;
using FluentMigrator;
using Newtonsoft.Json;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    //[Migration(2020_12_02_13_00)]
    public class M202012021300_AddTenantId : ForwardOnlyMigration
    {
        public override void Up()
        {
            var serviceKey = Guid.NewGuid();
            var serializeObject = JsonConvert.SerializeObject(new
            {
                TenantPublicId = serviceKey.ToString("N")
            });

            Execute.Sql($@"INSERT INTO appsettings (id, value) VALUES ('TenantSettings', '{serializeObject}')
                ON CONFLICT (id) DO NOTHING;");
        }
    }
}
