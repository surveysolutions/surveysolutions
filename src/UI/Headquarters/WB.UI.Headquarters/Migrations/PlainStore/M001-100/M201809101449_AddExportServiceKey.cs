using System;
using System.Security;
using FluentMigrator;
using Newtonsoft.Json;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(201809101449)]
    public class M201809101449_AddExportServiceKey : Migration
    {
        public override void Up()
        {
            var serviceKey = Guid.NewGuid();
            var serializeObject = JsonConvert.SerializeObject(new
            {
                Key = serviceKey.ToString()
            });
            Insert.IntoTable("appsettings").Row(new {id = "ExportService.ApiKey", value = serializeObject});
        }

        public override void Down()
        {
            Delete.FromTable("appsettings").Row(new {id = "ExportService.ApiKey"});
        }
    }
}
