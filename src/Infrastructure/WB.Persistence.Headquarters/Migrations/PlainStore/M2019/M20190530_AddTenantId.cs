using System;
using FluentMigrator;
using Newtonsoft.Json;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201905301715)]
    public class M201905301715_AddTenantId : AutoReversingMigration
    {
        public override void Up()
        {
            var serviceKey = Guid.NewGuid();
            var serializeObject = JsonConvert.SerializeObject(new
            {
                TenantPublicId = serviceKey.ToString("N")
            });
            Insert.IntoTable("appsettings").Row(new { id = "TenantSettings", value = serializeObject });
        }
    }
}
