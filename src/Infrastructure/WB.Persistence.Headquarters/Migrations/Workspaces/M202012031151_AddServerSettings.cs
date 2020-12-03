using System;
using Dapper;
using FluentMigrator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_12_03_11_51)]
    public class M202012031151_AddServerSettings : ForwardOnlyMigration
    {
        private const string ServerSettingsTableName = "server_settings";
        private readonly ILogger logger;

        public M202012031151_AddServerSettings(ILoggerProvider logger)
        {
            this.logger = logger.CreateLogger(GetType().Name);
        }
        
        public override void Up()
        {
            Create.Table(ServerSettingsTableName)
                .WithColumn("id").AsString().NotNullable().PrimaryKey()
                .WithColumn("value").AsCustom("jsonb").NotNullable();

            Execute.WithConnection((connection, transaction) =>
            {
                void Execute(string sql)
                {
                    logger.LogInformation(sql);
                    connection.Execute(sql);
                }
                
                if (connection.IsTableExistsInSchema("ws_primary", "appsettings"))
                {
                    Execute($@"INSERT INTO {ServerSettingsTableName} (id, value)
                SELECT 'TenantSettings', value from ws_primary.appsettings a 
                       where a.id = 'TenantSettings'
                ON CONFLICT (id) DO NOTHING;
                DELETE FROM ws_primary.appsettings where id = 'TenantSettings'");
                }
                else
                {
                    var serviceKey = Guid.NewGuid();
                    var serializeObject = JsonConvert.SerializeObject(new
                    {
                        TenantPublicId = serviceKey.ToString("N")
                    });
                    Execute($@"INSERT INTO {ServerSettingsTableName} (id, value) VALUES ('TenantSettings', '{serializeObject}')
                               ON CONFLICT (id) DO NOTHING;");
                }
            });
        }
    }
}
